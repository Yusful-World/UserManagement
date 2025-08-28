using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Services;
using UserManagement.Infrastructure.Services.Interfaces;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.CommandHandlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, BaseResponseDto<UserResponseDto>>
    {
        private readonly IMapper _mapper;
        private readonly ILogger<CreateUserCommandHandler> _logger;
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageService _imageService;

        public CreateUserCommandHandler(IMapper mapper, ILogger<CreateUserCommandHandler> logger,
            ITokenService tokenService, UserManager<User> userManager, 
            IHttpContextAccessor httpContextAccessor, IImageService imageService)
        {
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
        }


        public async Task<BaseResponseDto<UserResponseDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingEmail = await _userManager.FindByEmailAsync(request.CreateUser.Email);
                if (existingEmail is not null)
                {
                    return new FailureResponseDto<UserResponseDto>
                    {
                        Message = "Email already exists",
                        StatusCode = StatusCodes.Status409Conflict

                    };
                }

                var newUser = _mapper.Map<User>(request.CreateUser);

                if (newUser.Profile == null)
                    newUser.Profile = new UserProfile();
                newUser.UserName = request.CreateUser.Email;
                newUser.Profile.ProfilePic = await UploadImage(request);
                newUser.RefreshToken = _tokenService.GenerateRefreshToken();
                newUser.CreatedAt = DateTime.UtcNow;

                var createdUser = await _userManager.CreateAsync(newUser, request.CreateUser.Password);
                if (!createdUser.Succeeded)
                {
                    return new FailureResponseDto<UserResponseDto>
                    {
                        Message = "User creation failed",
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                if (newUser.Role == RolePermission.Admin)
                {
                    var createdUserRole = await _userManager.AddToRoleAsync(newUser, RoleName.Admin);
                    if (!createdUserRole.Succeeded)
                    {
                        return new FailureResponseDto<UserResponseDto>
                        {
                            Message = "Role assignment failed",
                            StatusCode = StatusCodes.Status500InternalServerError
                        };
                    }
                }
                else
                {
                    var createdUserRole = await _userManager.AddToRoleAsync(newUser, RoleName.User);
                    if (!createdUserRole.Succeeded)
                    {
                        return new FailureResponseDto<UserResponseDto>
                        {
                            Message = "Role assignment failed",
                            StatusCode = StatusCodes.Status500InternalServerError
                        };
                    }
                }


                var accessToken = _tokenService.GenerateToken(newUser, 20);

                UserResponseDto createdUserData = _mapper.Map<UserResponseDto>(newUser);

                createdUserData.AccessToken = accessToken;
                createdUserData.RefreshToken = newUser.RefreshToken;
                createdUserData.AccountType = newUser.Role.ToString();

                return new SuccessResponseDto<UserResponseDto>
                {
                    Message = "User registered successfully",
                    Data = createdUserData,
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user");
                return new FailureResponseDto<UserResponseDto>
                {
                    Message = "An error occurred while processing your request",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

        }

        public async Task<string?> UploadImage(CreateUserCommand request)
        {
            var profileImage = request.CreateUser.ProfilePic;
            string? imageUrl = null;
            if (profileImage is not null)
            {
                imageUrl = await _imageService.SaveImageAsync(profileImage);
            }

            return imageUrl;
        }
    }
}

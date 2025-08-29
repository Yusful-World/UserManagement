using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Infrastructure.Repository.Interfaces;
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
        private readonly IImageService _imageService;
        private readonly IRepository<UserProfile> _profileRepo;

        public CreateUserCommandHandler(IMapper mapper, ILogger<CreateUserCommandHandler> logger,
            ITokenService tokenService, UserManager<User> userManager,
            IImageService imageService, IRepository<UserProfile> profileRepo)
        {
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _userManager = userManager;
            _imageService = imageService;
            _profileRepo = profileRepo;
        }


        public async Task<BaseResponseDto<UserResponseDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check for existing user
                var existingEmail = await _userManager.FindByEmailAsync(request.CreateUser.Email);
                if (existingEmail is not null)
                {
                    return new FailureResponseDto<UserResponseDto>
                    {
                        Message = "Email already exists",
                        StatusCode = StatusCodes.Status409Conflict

                    };
                }

                //map to entity
                var newUser = _mapper.Map<User>(request.CreateUser);               
                newUser.UserName = request.CreateUser.Email;
                newUser.RefreshToken = _tokenService.GenerateRefreshToken();
                newUser.IsActive = true;
                newUser.CreatedAt = DateTime.UtcNow;

                //create user
                var createdUser = await _userManager.CreateAsync(newUser, request.CreateUser.Password);
                if (!createdUser.Succeeded)
                {
                    return new FailureResponseDto<UserResponseDto>
                    {
                        Message = "User creation failed",
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                //assign role
                var roleName = newUser.Role == RolePermission.Admin ? RoleName.Admin : RoleName.User;
                var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);
                if (!roleResult.Succeeded)
                {
                    return new FailureResponseDto<UserResponseDto>
                    {
                        Message = "Role assignment failed",
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }

                //build profile
                newUser.Profile ??= await BuildProfile(request, newUser.Id);
                await _profileRepo.AddAsync(newUser.Profile);
                await _profileRepo.SaveChangesAsync(cancellationToken);


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

        private async Task<UserProfile> BuildProfile(CreateUserCommand request, Guid userId)
        {
            return new UserProfile()
            {
                ProfilePic = request?.CreateUser.ProfilePic is not null ? await UploadImage(request) : null,
                DateOfBirth = request?.CreateUser?.DateOfBirth ?? default,
                Nationality = request?.CreateUser?.Nationality ?? string.Empty,
                Gender = Enum.GetName(typeof(Gender), request?.CreateUser?.Gender),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
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

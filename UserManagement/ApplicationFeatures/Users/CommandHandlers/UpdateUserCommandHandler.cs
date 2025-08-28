using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Repository.Interfaces;
using UserManagement.Infrastructure.Services.Interfaces;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.CommandHandlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, BaseResponseDto<UpdateProfileResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<UserProfile> _profileRepo;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IImageService _imageService;

        public UpdateUserCommandHandler(
            UserManager<User> userManager, IRepository<UserProfile> profileRepo,
            IMapper mapper, ITokenService tokenService,
            IConfiguration configuration, IImageService imageService)
        {
            _userManager = userManager;
            _profileRepo = profileRepo;
            _mapper = mapper;
            _tokenService = tokenService;
            _configuration = configuration;
            _imageService = imageService;
        }

        public async Task<BaseResponseDto<UpdateProfileResponseDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
                return new FailureResponseDto<UpdateProfileResponseDto> { Message = "User not found", StatusCode = StatusCodes.Status404NotFound };

            if (user.Profile == null)
            {
                user.Profile = await BuildProfile(request, user.Id);
                await _profileRepo.AddAsync(user.Profile);

            }
            else
            {
                user.Profile = await UpdateProfile(user, request);
            }

            user = UpdateUser(user, request);

            await _userManager.UpdateAsync(user);
            await _profileRepo.SaveChangesAsync(cancellationToken);

            var profileDto = _mapper.Map<UpdateProfileResponseDto>(user.Profile);

            return new SuccessResponseDto<UpdateProfileResponseDto>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Profile update successful",
                Data = profileDto
            };
        }
        private static User UpdateUser(User user, UpdateUserCommand request)
        {
            user.PhoneNumber = !string.IsNullOrWhiteSpace(request.UpdateRequest.PhoneNumber) ? request.UpdateRequest.PhoneNumber : user.PhoneNumber;
            user.FirstName = !string.IsNullOrWhiteSpace(request.UpdateRequest.FirstName) ? request.UpdateRequest.FirstName : user.FirstName;
            user.LastName = !string.IsNullOrWhiteSpace(request.UpdateRequest.LastName) ? request.UpdateRequest.LastName : user.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            return user;
        }

        private async Task<UserProfile> UpdateProfile(User user, UpdateUserCommand request)
        {
            user.Profile.ProfilePic = (request.UpdateRequest.ProfilePic != null) ? await UploadImage(request) : user.Profile.ProfilePic;
            user.Profile.Address = !string.IsNullOrWhiteSpace(request.UpdateRequest.Address) ? request.UpdateRequest.Address : user.Profile.Address;
            user.Profile.FacebookLink = !string.IsNullOrWhiteSpace(request.UpdateRequest.FacebookLink) ? request.UpdateRequest.FacebookLink : user.Profile.FacebookLink;
            user.Profile.LinkedinLink = !string.IsNullOrWhiteSpace(request.UpdateRequest.LinkedinLink) ? request.UpdateRequest.LinkedinLink : user.Profile.LinkedinLink;
            user.Profile.TwitterLink = !string.IsNullOrWhiteSpace(request.UpdateRequest.TwitterLink) ? request.UpdateRequest.TwitterLink : user.Profile.TwitterLink;
            user.Profile.InstagramLink = !string.IsNullOrWhiteSpace(request.UpdateRequest.InstagramLink) ? request.UpdateRequest.InstagramLink : user.Profile.InstagramLink;
            user.Profile.UpdatedAt = DateTime.UtcNow;


            return user.Profile;
        }

        private async Task<UserProfile> BuildProfile(UpdateUserCommand request, Guid userId)
        {
            return new UserProfile()
            {
                ProfilePic = await UploadImage(request),
                Address = request.UpdateRequest.Address,
                FacebookLink = request.UpdateRequest.FacebookLink,
                LinkedinLink = request.UpdateRequest.LinkedinLink,
                TwitterLink = request.UpdateRequest.TwitterLink,
                InstagramLink = request.UpdateRequest.InstagramLink,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<string?> UploadImage(UpdateUserCommand request)
        {
            var profileImage = request.UpdateRequest.ProfilePic;
            string? imageUrl = null;
            if (profileImage is not null)
            {
                using var stream = profileImage.OpenReadStream();
                imageUrl = await _imageService.SaveImageAsync(profileImage);
            }

            return imageUrl;
        }
    }
}

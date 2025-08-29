using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
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
        private readonly IImageService _imageService;

        public UpdateUserCommandHandler(
            UserManager<User> userManager,
            IRepository<UserProfile> profileRepo,
            IMapper mapper,
            IImageService imageService)
        {
            _userManager = userManager;
            _profileRepo = profileRepo;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<BaseResponseDto<UpdateProfileResponseDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (user == null)
            {
                return new FailureResponseDto<UpdateProfileResponseDto>
                {
                    Message = "User not found",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Update base user properties
            UpdateUser(user, request);

            //ensure profile exists
            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _profileRepo.AddAsync(user.Profile);
            }

            await UpdateProfile(user, request);

            await _userManager.UpdateAsync(user);
            await _profileRepo.SaveChangesAsync(cancellationToken);

            // map to profile Dto
            var profileDto = _mapper.Map<UpdateProfileResponseDto>(user.Profile);
            profileDto.FirstName = user.FirstName;
            profileDto.LastName = user.LastName;
            profileDto.PhoneNumber = user.PhoneNumber;

            return new SuccessResponseDto<UpdateProfileResponseDto>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Profile updated successfully",
                Data = profileDto
            };
        }

        private static void UpdateUser(User user, UpdateUserCommand request)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.UpdateRequest.PhoneNumber)
                ? user.PhoneNumber
                : request.UpdateRequest.PhoneNumber;

            user.FirstName = string.IsNullOrWhiteSpace(request.UpdateRequest.FirstName)
                ? user.FirstName
                : request.UpdateRequest.FirstName;

            user.LastName = string.IsNullOrWhiteSpace(request.UpdateRequest.LastName)
                ? user.LastName
                : request.UpdateRequest.LastName;

            user.UpdatedAt = DateTime.UtcNow;
        }

        private async Task UpdateProfile(User user, UpdateUserCommand request)
        {
            var profile = user.Profile;

            // Handle image update (replace existing if needed)
            if (request.UpdateRequest.ProfilePic is not null)
            {
                profile.ProfilePic = await UpdateImage(profile.ProfilePic, request.UpdateRequest.ProfilePic);
            }

            profile.Address = string.IsNullOrWhiteSpace(request.UpdateRequest.Address)
                ? profile.Address
                : request.UpdateRequest.Address;

            profile.Gender = Enum.GetName(typeof(Gender), request?.UpdateRequest?.Gender);
            profile.DateOfBirth = (request.UpdateRequest.DateOfBirth is null) 
                ? profile.DateOfBirth 
                : request.UpdateRequest.DateOfBirth;

            profile.StateOfOrigin = string.IsNullOrWhiteSpace(request.UpdateRequest.StateOfOrigin)
                ? profile.StateOfOrigin
                : request.UpdateRequest.StateOfOrigin;

            profile.Nationality = string.IsNullOrWhiteSpace(request.UpdateRequest.Nationality)
                ? profile.Nationality
                : request.UpdateRequest.Nationality;

            profile.FacebookLink = string.IsNullOrWhiteSpace(request.UpdateRequest.FacebookLink)
                ? profile.FacebookLink
                : request.UpdateRequest.FacebookLink;

            profile.TwitterLink = string.IsNullOrWhiteSpace(request.UpdateRequest.TwitterLink)
                ? profile.TwitterLink
                : request.UpdateRequest.TwitterLink;

            profile.LinkedinLink = string.IsNullOrWhiteSpace(request.UpdateRequest.LinkedinLink)
                ? profile.LinkedinLink
                : request.UpdateRequest.LinkedinLink;

            profile.InstagramLink = string.IsNullOrWhiteSpace(request.UpdateRequest.InstagramLink)
                ? profile.InstagramLink
                : request.UpdateRequest.InstagramLink;

            profile.UpdatedAt = DateTime.UtcNow;
        }

        private async Task<string?> UpdateImage(string? existingImageUrl, IFormFile newImage)
        {
            if (newImage == null) return existingImageUrl;

            // If user already had an image, delete it first
            if (!string.IsNullOrWhiteSpace(existingImageUrl))
            {
                await _imageService.DeleteImageAsync(existingImageUrl);
            }

            // Upload new one
            return await _imageService.SaveImageAsync(newImage);
        }
    }

}

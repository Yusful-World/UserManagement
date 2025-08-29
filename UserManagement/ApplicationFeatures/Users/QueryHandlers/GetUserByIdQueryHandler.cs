using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.ApplicationFeatures.Users.Queries;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.QueryHandlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, BaseResponseDto<UserResponseDto>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public GetUserByIdQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<BaseResponseDto<UserResponseDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                return new FailureResponseDto<UserResponseDto>
                {
                    Message = $"User with id {request.UserId} not found",
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userResponseDto = _mapper.Map<UserResponseDto>(user);

            userResponseDto.AccountType = roles.FirstOrDefault();


            return new SuccessResponseDto<UserResponseDto>
            {
                Message = "User fetched successfully",
                Data = userResponseDto
            };
        }
    }
}

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
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsers, IEnumerable<BaseResponseDto<UserResponseDto>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BaseResponseDto<UserResponseDto>>> Handle(GetAllUsers request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users
                .Include(u => u.Profile)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var dto = _mapper.Map<UserResponseDto>(user);

                var roles = await _userManager.GetRolesAsync(user);
                dto.AccountType = roles.FirstOrDefault();

                userDtos.Add(dto);
            }


            return userDtos.Select(dto => new SuccessResponseDto<UserResponseDto>
            {
                Message = "Users retrieved successfully",
                Data = dto
            });
        }
    }
}

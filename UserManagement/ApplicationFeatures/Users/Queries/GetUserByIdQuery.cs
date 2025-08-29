using MediatR;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.Queries
{
    public class GetUserByIdQuery : IRequest<BaseResponseDto<UserResponseDto>>
    {
        public Guid UserId { get; set; }

        public GetUserByIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}

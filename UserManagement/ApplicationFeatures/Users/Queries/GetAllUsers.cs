using MediatR;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.Queries
{
    public class GetAllUsers : IRequest<IEnumerable<BaseResponseDto<UserResponseDto>>>
    {

    }
}

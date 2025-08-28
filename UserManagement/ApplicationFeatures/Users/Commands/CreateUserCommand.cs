using MediatR;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.Commands
{
    public class CreateUserCommand : IRequest<BaseResponseDto<UserResponseDto>>
    {
        public CreateUserRequestDto CreateUser { get; set; }
        public CreateUserCommand(CreateUserRequestDto createUser)
        {
            CreateUser = createUser;
        }
    }
}

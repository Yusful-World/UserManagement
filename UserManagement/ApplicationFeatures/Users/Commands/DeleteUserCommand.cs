using MediatR;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.Commands
{
    public record DeleteUserCommand : IRequest<BaseResponseDto<object>>
    {
        public IEnumerable<Guid> Id { get; set; }
        public DeleteUserCommand(IEnumerable<Guid> id)
        {
            Id = id;
        }
    }
}

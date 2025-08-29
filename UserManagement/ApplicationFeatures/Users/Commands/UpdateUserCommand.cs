using MediatR;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.Commands
{
    public class UpdateUserCommand : IRequest<BaseResponseDto<UpdateProfileResponseDto>>
    {
        public Guid Id { get; set; }
        public UpdateProfileRequestDto UpdateRequest { get; set; }
        public UpdateUserCommand(Guid id, UpdateProfileRequestDto updateRequest)
        { 
            UpdateRequest = updateRequest;
            Id = id;
        }
    }
}

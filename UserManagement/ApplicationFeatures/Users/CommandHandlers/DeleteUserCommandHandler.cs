using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.SharedDtos;

namespace UserManagement.ApplicationFeatures.Users.CommandHandlers
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, BaseResponseDto<object>>
    {
        private readonly UserManager<User> _userManager;

        public DeleteUserCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<BaseResponseDto<object>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var failedDeletions = new List<string>();

            foreach (var id in request.Id)
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    failedDeletions.Add($"User with ID {id} not found.");
                    continue;
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    failedDeletions.AddRange(result.Errors.Select(e => $"User {id}: {e.Description}"));
                }
            }

            if (failedDeletions.Any())
            {
                return new FailureResponseDto<object>
                {
                    Message = "Some deletions failed.",
                    Data = failedDeletions,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            return new SuccessResponseDto<object>
            {
                Message = "All users deleted successfully.",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}

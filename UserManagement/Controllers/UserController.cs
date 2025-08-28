using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.ApplicationFeatures.Users.Commands;
using UserManagement.ApplicationFeatures.Users.Dtos;
using UserManagement.ApplicationFeatures.Users.Queries;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create-user")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserRequestDto request)
        {
            var command = new CreateUserCommand(request);
            var response = await _mediator.Send(command);

            if (response.Data == null)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(CreateUser), response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
        {
            var query = new GetUserByIdQuery(id);
            var response = await _mediator.Send(query);
            if (response.Data == null)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("delete_users")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> DeleteRangeUser(IEnumerable<Guid> id)
        {
            var command = new DeleteUserCommand(id);
            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPatch("update-user{id}")]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UpdateProfileResponseDto>> UpdateProfile(string id, [FromForm] UpdateProfileRequestDto requestDto)
        {
            var command = new UpdateUserCommand(id, requestDto);
            var response = await _mediator.Send(command);

            return Ok(response);
        }
    }
}

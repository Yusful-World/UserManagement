using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.SharedDtos
{
    public class FailureResponseDto<T> : BaseResponseDto<T>
    {

        public FailureResponseDto()
        {
            Message = "Request failed";
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}

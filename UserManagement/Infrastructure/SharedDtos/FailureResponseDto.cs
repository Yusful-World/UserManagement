using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.SharedDtos
{
    public class FailureResponseDto<T> : BaseResponseDto<T>
    {
        public string Error { get; set; }

        public FailureResponseDto()
        {
            Message = "Request failed";
            StatusCode = StatusCodes.Status400BadRequest;
            
        }
    }
}

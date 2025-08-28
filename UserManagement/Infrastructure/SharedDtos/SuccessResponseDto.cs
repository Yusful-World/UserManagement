using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.SharedDtos
{
    public class SuccessResponseDto<T> : BaseResponseDto<T>
    {
        public SuccessResponseDto()
        {
            Message = "Request successful";
            StatusCode = StatusCodes.Status200OK;
        }
    }
}

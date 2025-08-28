using System.Text.Json.Serialization;

namespace UserManagement.Infrastructure.SharedDtos
{
    public class BaseResponseDto<T>
    {

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
    }
}

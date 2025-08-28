using System.Text.Json.Serialization;
using UserManagement.Domain.Enums;

namespace UserManagement.ApplicationFeatures.Users.Dtos
{
    public class UserResponseDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = "";

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("is_superadmin")]
        public bool IsSuperAdmin { get; set; }

        [JsonPropertyName("account_type")]
        public string? AccountType { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? Nationality { get; set; }
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("profile")]
        public UpdateProfileDto? Profile { get; set; }
    }
}

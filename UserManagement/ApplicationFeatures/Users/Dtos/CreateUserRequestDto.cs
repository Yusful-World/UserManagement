using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UserManagement.Domain.Enums;

namespace UserManagement.ApplicationFeatures.Users.Dtos
{
    public record CreateUserRequestDto
    {
        [JsonPropertyName("first_name")]
        [StringLength(55, ErrorMessage = "First name cannot exceed 55 characters")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        [StringLength(55, ErrorMessage = "Last name cannot exceed 55 characters")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [JsonPropertyName("phone_number")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain at least one letter and one number")]
        public string Password { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? Nationality { get; set; }
        public IFormFile? ProfilePic { get; set; }
    }
}

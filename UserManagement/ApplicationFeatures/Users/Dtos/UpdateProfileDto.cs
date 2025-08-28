using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.ApplicationFeatures.Users.Dtos
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? StateOfOrigin { get; set; }
        public string? Nationality { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string? FacebookLink { get; set; }
        public string? TwitterLink { get; set; }
        public string? LinkedinLink { get; set; }
        public string? InstagramLink { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

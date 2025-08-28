using System.ComponentModel.DataAnnotations;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.ApplicationFeatures.Users.Dtos
{
    public class UpdateProfileRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? StateOfOrigin { get; set; }
        public string? Nationality { get; set; }
        public IFormFile? ProfilePic { get; set; }
        public string? FacebookLink { get; set; }
        public string? TwitterLink { get; set; }
        public string? LinkedinLink { get; set; }
        public string? InstagramLink { get; set; }
    }
}

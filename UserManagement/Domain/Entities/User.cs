using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Enums;

namespace UserManagement.Domain.Entities
{
    public class User : IdentityUser<Guid>, IEntityBase
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Password { get; set; }
        public UserProfile Profile { get; set; }
        public string? RefreshToken { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public RolePermission RolePermission { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSuperAdmin { get; set; }
    }
}

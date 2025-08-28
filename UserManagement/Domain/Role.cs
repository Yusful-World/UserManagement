using Microsoft.AspNetCore.Identity;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain
{
    public class Role : IdentityRole<Guid>, IEntityBase
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

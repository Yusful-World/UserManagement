using Microsoft.AspNetCore.Identity;

namespace UserManagement.Domain.Entities
{
    public class Role : IdentityRole<Guid>, IEntityBase
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

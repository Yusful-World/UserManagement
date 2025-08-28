using System.Security.Claims;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(User user, int expireInMinutes);

        public string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

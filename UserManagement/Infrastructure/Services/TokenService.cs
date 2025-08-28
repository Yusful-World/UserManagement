using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Services.Interfaces;
using UserManagement.Infrastructure.Utils;

namespace UserManagement.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly Jwt _jwtSettings;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<User> _userManager;

        public TokenService(IOptions<Jwt> jwtOptions, IHttpContextAccessor contextAccessor)
        {
            _jwtSettings = jwtOptions.Value;
            _securityKey = GetSecurityKey(_jwtSettings.SecretKey);
            _contextAccessor = contextAccessor;
        }
        public string GenerateToken(User user, int expireInMinutes = 0)
        {
            var signingCredential = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            Claim[] claims = [
                new(ClaimTypes.Sid, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FirstName)
            ];

            expireInMinutes = expireInMinutes == 0 ? _jwtSettings.ExpireInMinutes : expireInMinutes;
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(expireInMinutes),
                Issuer = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer) ? _jwtSettings.Issuer : "",
                Audience = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer) ? _jwtSettings.Issuer : "",
                SigningCredentials = signingCredential,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            var randomNumber = RandomNumberGenerator.Create();
            randomNumber.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer) ? _jwtSettings.Issuer : "",
                ValidAudience = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer) ? _jwtSettings.Issuer : "",
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(10),
                IssuerSigningKey = _securityKey
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private static SymmetricSecurityKey GetSecurityKey(string secretKey)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }
    }
}

namespace UserManagement.Infrastructure.Utils
{
    public class Jwt
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int ExpireInMinutes { get; set; }
    }
}

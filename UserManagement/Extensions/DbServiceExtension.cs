using Microsoft.EntityFrameworkCore;
using System.Runtime;
using UserManagement.Data;
using UserManagement.Infrastructure.Utils;

namespace UserManagement.Extensions
{
    public static class DbServiceExtension
    {
        public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("FATAL ERROR: The 'DefaultConnection' string is null or empty.");
                
                return null;
            }

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "PostgreSQL",
                tags: new[] { "db", "sql" }
            );

            return services;
        }
    }
}

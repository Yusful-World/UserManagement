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
            var dbSettings = configuration.GetSection("DbCredentials").Get<DbCredentials>()
            ?? throw new InvalidOperationException("DbCredentials section is missing.");

            var connectionString = BuildConnectionString(dbSettings);

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            //services.AddHealthChecks()
            //.AddNpgSql(
            //    connectionString,
            //    name: "PostgreSQL",
            //    tags: new[] { "db", "sql" }
            //);

            return services;
        }


        private static string BuildConnectionString(DbCredentials credentials)
        {
            return $"Server={credentials.Server};Port={credentials.Port};Database={credentials.Database};User Id={credentials.Username};Password={credentials.Password};";
        }
    }
}

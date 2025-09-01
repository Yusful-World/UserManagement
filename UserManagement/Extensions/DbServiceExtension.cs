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
            //var connectionString = configuration["DefaultConnection"];
            
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
                "postgresql://lafargeusermanagement_user:uneAFJMyRATLJlblZ3j6g4Ibojmrb1pE@dpg-d2os2kh5pdvs73cusn6g-a/lafargeusermanagement")
           );

            return services;
        }

    }
}

using MailKit;
using Microsoft.AspNetCore.Identity;
using UserManagement.Data.Interfaces;
using UserManagement.Data;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Repository.Interfaces;
using UserManagement.Infrastructure.Repository;
using UserManagement.Infrastructure.Services.Interfaces;
using UserManagement.Infrastructure.Services;

namespace UserManagement.Extensions
{
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructureConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConnectionString(configuration);
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddScoped<ITokenService, TokenService>();


            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IImageService, ImageService>();


            return services;
        }
    }
}

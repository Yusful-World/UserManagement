using MailKit;
using Microsoft.AspNetCore.Identity;
using UserManagement.Data.Interfaces;
using UserManagement.Data;
using UserManagement.Domain.Entities;

namespace UserManagement.Extensions
{
    public static class InfrastructureServiceExtension
    {
        public static IServiceCollection AddInfrastructureConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConnectionString(configuration);
            services.AddScoped<IDbInitializer, DbInitializer>();
            //services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //services.AddScoped<IEmailService, EmailService>();
            //services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            //services.AddScoped<IMessageQueueService, MessageQueueService>();
            //services.AddSingleton(configuration.GetSection("SMTP_CREDENTIALS").Get<SmtpCredentials>());

            //services.AddScoped<ITokenService, TokenService>();
            //services.AddHostedService<TokenCleanUpService>();

            //services.AddSingleton<IBackgroundTaskService, BackgroundTaskService>();
            //services.AddHostedService<EmailBackgroundService>();

            //services.AddScoped<ICloudinaryService, CloudinaryService>();
            //services.AddScoped<IImageService, ImageService>();




            return services;
        }
    }
}

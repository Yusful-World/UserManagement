using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using UserManagement.ApplicationFeatures.Users.Mappers;
using UserManagement.Data;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Utils;

namespace UserManagement.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                );
            });

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddHealthChecks();
            services.AddControllers().AddNewtonsoftJson(options =>
            { options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; }
            );

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            //services.AddApiVersioning();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new UserMappingProfile());
            }, Assembly.GetExecutingAssembly());
            services.Configure<Jwt>(configuration.GetSection("JWT"));

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            var jwtSettings = configuration.GetSection("JWT").Get<Jwt>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                options.DefaultSignInScheme =
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer) ? jwtSettings.Issuer : "",
                    ValidAudience = !string.IsNullOrWhiteSpace(jwtSettings.Issuer) ? jwtSettings.Issuer : "",
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(10),
                    IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
        )
                };
            });

            
            services.AddAuthorization();

            return services;
        }
    }
}

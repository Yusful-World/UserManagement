using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;

namespace UserManagement.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AppDbContext _db;
        private readonly ILogger<DbInitializer> _logger;
        private readonly IConfiguration _configuration;
        private readonly string[] _roles =
        [
            RoleName.User,
            RoleName.Admin,
        ];

        public DbInitializer(
            UserManager<User> userManager, RoleManager<Role> roleManager,
            AppDbContext db, ILogger<DbInitializer> logger,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _logger = logger;
            _configuration = configuration;
        }

        public string[] Roles => _roles;

        public async Task InitializeAsync()
        {
            //migrations if they are not applied
            if (_configuration.GetValue<bool>("AllowMigrations"))
            {
                try
                {
                    if (_db.Database.GetPendingMigrations().Any())
                    {
                        _logger.LogInformation("Applying database migration...");
                        await _db.Database.MigrateAsync();
                        _logger.LogInformation("Database migrations applied successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while applying database migrations.");
                }
            }
            else
            {
                _logger.LogInformation("Database migrations skipped based on configuration ('AllowMigrations' is false).");
            }


            //create roles if they are not created
            foreach (var role in Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new Role { Name = role });
            }

            //if roles are not created, then we will create admin user as well
            var adminEmail = "Abuyusuf182@gmail.com";
            var existingUser = await _userManager.FindByEmailAsync(adminEmail);
            if (existingUser == null)
            {
                var adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Yusuf",
                    LastName = "Abdullahi",
                    PhoneNumber = "08053565771",
                    RolePermission = RolePermission.Admin,
                    IsSuperAdmin = true,
                };
                var result = await _userManager.CreateAsync(adminUser, adminEmail);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, RoleName.Admin);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create admin user: {Errors}", errors);
                }
            }

            return;
        }
    }
}

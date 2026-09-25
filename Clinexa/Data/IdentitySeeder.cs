using Clinexa.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<Role>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<User>>();

            var db =
                serviceProvider.GetRequiredService<Clinexa.Data.AppDbContext>();


            // =========================
            // Ensure Roles Exist
            // =========================

            string[] roles =
            {
                "Admin",
                "Doctor",
                "Receptionist"
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role
                    {
                        Name = roleName,
                        Description = $"{roleName} role"
                    };

                    await roleManager.CreateAsync(role);
                }
            }


            // =========================
            // Fix Existing Users
            // =========================

            var users = await db.Users.ToListAsync();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    user.SecurityStamp = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(user.ConcurrencyStamp))
                {
                    user.ConcurrencyStamp = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(user.UserName))
                {
                    user.UserName = user.Email;
                }

                if (string.IsNullOrEmpty(user.NormalizedUserName))
                {
                    user.NormalizedUserName =
                        user.UserName?.ToUpperInvariant();
                }

                if (string.IsNullOrEmpty(user.NormalizedEmail))
                {
                    user.NormalizedEmail =
                        user.Email?.ToUpperInvariant();
                }
            }

            // Save directly through EF Core.
            // We intentionally do NOT use UserManager.UpdateAsync()
            // because these users have NULL SecurityStamp values.
            await db.SaveChangesAsync();


            // =========================
            // Ensure Admin User Exists
            // =========================

            var adminEmail = "admin@clinexa.com";

            var admin =
                await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    PhoneNumber = "01000000000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result =
                    await userManager.CreateAsync(
                        admin,
                        "Admin@12345");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        "Admin");
                }
            }
        }
    }
}
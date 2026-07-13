using Microsoft.AspNetCore.Identity;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.SeedingData
{
    public static class SeedingAccounts
    {
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string adminEmail = "Admin@Gmail.com";
            const string adminPassword = "Admin@123456";

            var adminExists = await userManager.FindByEmailAsync(adminEmail);
            if (adminExists is not null)
                return;

            var admin = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Admin",
                UserName = "SystemAdmin",
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new Exception($"Failed to seed admin account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(admin, "SuperAdmin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Admin role: {errors}");
            }
        }
    }
}
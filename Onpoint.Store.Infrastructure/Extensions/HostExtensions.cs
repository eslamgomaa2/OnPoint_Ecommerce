using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Infrastructure.Data.Context;
using Onpoint.Store.Infrastructure.SeedingData;

namespace Onpoint.Store.Infrastructure.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> MigrateDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DatabaseMigration");

            try
            {
                logger.LogInformation("Starting migration...");

                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migration completed.");

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                await SeedingRoles.SeedRolesAsync(roleManager);
                logger.LogInformation("Roles seeded successfully");

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                await SeedingAccounts.SeedAdminAsync(userManager, dbContext);
                logger.LogInformation("Default accounts seeded successfully");
                await SeedingAccounts.SeedCashierAsync(userManager, dbContext);
                logger.LogInformation("Default accounts seeded successfully");


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration or seeding.");
                throw;
            }

            return host;
        }
    }
}
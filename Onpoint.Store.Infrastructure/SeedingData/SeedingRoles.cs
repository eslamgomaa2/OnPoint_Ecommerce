using Microsoft.AspNetCore.Identity;

namespace Onpoint.Store.Infrastructure.SeedingData
{
    internal class SeedingRoles
    {
        private static readonly string[] Roles = { "SuperAdmin", "Customer", "Cashier", "BranchManager" };

        public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
        }
    }
}

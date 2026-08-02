using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.SeedingData
{
    public static class SeedingAccounts
    {
        public static async Task SeedAppleAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string appleEmail = "apple@gmail.com";
            const string applePassword = "Abood@2002";

            var appleExists = await userManager.FindByEmailAsync(appleEmail);
            if (appleExists is not null)
                return;

            var apple = new ApplicationUser
            {
                FirstName = "Apple",
                LastName = "Account",
                UserName = "Apple_Account",
                Email = appleEmail,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(apple, applePassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new Exception($"Failed to seed apple account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(apple, "Customer");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Customer role: {errors}");
            }
        }
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
        public static async Task SeedONPointManagerAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string ONPointManagerEmail = "ONPointManager@Gmail.com";
            const string ONPointManagerPassword = "ONPointManager@123456";

            var ONPointManagerExists = await userManager.FindByEmailAsync(ONPointManagerEmail);
            if (ONPointManagerExists is not null)
                return;

            var ONPointManager = new ApplicationUser
            {
                FirstName = "OnPoint",
                LastName = "Manager",
                UserName = "ONPoint_Manager",
                Email = ONPointManagerEmail,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(ONPointManager, ONPointManagerPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new Exception($"Failed to seed ONPointManager account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(ONPointManager, "ONPointManager");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign ONPointManager role: {errors}");
            }
        }


        public static async Task SeedCashierAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string cashierEmail = "Cashier@Gmail.com";
            const string cashierPassword = "Cashier@123456";

            var cashierExists = await userManager.FindByEmailAsync(cashierEmail);
            if (cashierExists is not null)
                return;


            var defaultBranch = await context.Branches.FirstOrDefaultAsync(b => b.IsDefault);
            if (defaultBranch is null)
            {
                defaultBranch = new Branch
                {
                    Name = "Main Branch",
                    IsActive = true,
                    IsDefault = true
                };
                context.Branches.Add(defaultBranch);
                await context.SaveChangesAsync();
            }

            var cashier = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Cashier",
                UserName = "SystemCashier",
                Email = cashierEmail,
                EmailConfirmed = true,
                IsActive = true,
                BranchId = defaultBranch.Id,
                BranchRole = UserBranchRole.Cashier

            };

            var result = await userManager.CreateAsync(cashier, cashierPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed cashier account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(cashier, "Cashier");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Cashier role: {errors}");
            }
        }
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string UserEmail = "User@Gmail.com";
            const string UserPassword = "User@123456";

            var userExists = await userManager.FindByEmailAsync(UserEmail);
            if (userExists is not null)
                return;



            var user = new ApplicationUser
            {
                FirstName = "User",
                LastName = "test",
                UserName = "User_test",
                Email = UserEmail,
                EmailConfirmed = true,
                IsActive = true,

            };

            var result = await userManager.CreateAsync(user, UserPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed user account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign Customer role: {errors}");
            }
        }
        public static async Task SeedBranchManagerAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            const string BranchManagerEmail = "BranchManager@Gmail.com";
            const string BranchManagerPassword = "BranchManager@123456";

            var userExists = await userManager.FindByEmailAsync(BranchManagerEmail);
            if (userExists is not null)
                return;

            var defaultBranch = await context.Branches.FirstOrDefaultAsync(b => b.IsDefault);
            if (defaultBranch is null)
            {
                defaultBranch = new Branch
                {
                    Name = "Main Branch",
                    IsActive = true,
                    IsDefault = true,
                    Phone = "123456",
                    Email = "BranchManager@Gmail.com"
                };
                context.Branches.Add(defaultBranch);
                await context.SaveChangesAsync();
            }

            var branchManager = new ApplicationUser
            {
                FirstName = "Branch",
                LastName = "Manager",
                UserName = "BranchManager",
                Email = BranchManagerEmail,
                EmailConfirmed = true,
                IsActive = true,
                BranchRole = UserBranchRole.Manager,
                BranchId = defaultBranch.Id
            };

            var result = await userManager.CreateAsync(branchManager, BranchManagerPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to seed branch manager account: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(branchManager, "BranchManager");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to assign BranchManager role: {errors}");
            }


            defaultBranch.ManagerId = branchManager.Id;
            context.Branches.Update(defaultBranch);
            await context.SaveChangesAsync();
        }

    }
}
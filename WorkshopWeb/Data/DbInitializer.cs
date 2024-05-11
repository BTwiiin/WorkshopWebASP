using Microsoft.AspNetCore.Identity;
using WorkshopWeb.Models;

namespace WorkshopWeb.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(UserManager<CustomUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new CustomUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    Name = "Admin",
                    Surname = "User",
                    EmailConfirmed = true,
                    Role=ENum.Role.Admin
                };

                var createAdmin = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

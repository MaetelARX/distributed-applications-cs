using BookProject.Constants;
using Microsoft.AspNetCore.Identity;

namespace BookProject.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultDataAsync(IServiceProvider service)
        {
            var userMgr = service.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = service.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleMgr.RoleExistsAsync(Roles.Admin.ToString()))
            {
                var roleResult = await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"Error creating role Admin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await roleMgr.RoleExistsAsync(Roles.User.ToString()))
            {
                var roleResult = await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"Error creating role User: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            var admin = new IdentityUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true
            };

            var userInDb = await userMgr.FindByEmailAsync(admin.Email);
            if (userInDb is null)
            {
                var createAdminResult = await userMgr.CreateAsync(admin, "Admin@123");
                if (createAdminResult.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
                }
                else
                {
                    foreach (var error in createAdminResult.Errors)
                    {
                        Console.WriteLine($"Error creating admin: {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }


    }
}

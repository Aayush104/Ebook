using BookProject.Model;
using Microsoft.AspNetCore.Identity;

namespace BookProject.Data
{
    public static class DbSeed
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Roles with new GUIDs
            var roles = new List<(string Id, string Name)>
            {
                ("a1b2c3d4-e5f6-4789-abcd-1234567890ab", "Admin"),
                ("b2c3d4e5-f6a7-4890-bcde-2345678901bc", "Staff"),
                ("c3d4e5f6-a7b8-4901-cdef-3456789012cd", "PublicUser"),
            };

            foreach (var (id, name) in roles)
            {
                var existingRole = await roleManager.FindByNameAsync(name);
                if (existingRole == null)
                {
                    await roleManager.CreateAsync(new IdentityRole
                    {
                        Id = id,
                        Name = name,
                        NormalizedName = name.ToUpper()
                    });
                }
            }

            var adminId = "d4e5f6a7-b8c9-4012-def0-4567890123de";
            var adminEmail = "Admin987@gmail.com";
            var adminPassword = "Admin@987";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    Id = adminId,
                    UserName = "Admin",
                    NormalizedUserName = "ADMIN",
                    FullName = "Admin",
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = "9807083727"
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
            else
            {
                
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

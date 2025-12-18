using Lab5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lab5.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Створимо ролі
            string[] roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Створимо адміна
            var adminEmail = "admin@example.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Створимо простого користувача
            var userEmail = "user@example.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new ApplicationUser { UserName = userEmail, Email = userEmail };
                await userManager.CreateAsync(user, "User123!");
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}

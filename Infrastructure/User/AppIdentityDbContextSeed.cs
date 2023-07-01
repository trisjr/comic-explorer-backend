using ApplicationCore.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.User;

public static class AppIdentityDbContextSeed
{
    private const string DefaultPassword = "Pass@word1";

    public static async Task SeedAsync(
        AppIdentityDbContext identityDbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        if (identityDbContext.Database.IsRelational())
            await identityDbContext.Database.MigrateAsync();
        if (!await roleManager.RoleExistsAsync(ApplicationRoles.Administrator))
        {
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Administrator));
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.User));
            await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Manager));
        }

        var basicUserName = "user@email.com";
        if (await userManager.FindByNameAsync(basicUserName) == null)
        {
            var defaultUser = new ApplicationUser
            {
                UserName = basicUserName,
                Email = basicUserName,
                FirstName = "Demo",
                LastName = "User",
            };
            await userManager.CreateAsync(defaultUser, DefaultPassword);
            defaultUser = await userManager.FindByNameAsync("user@email.com");
            if (defaultUser != null)
                await userManager.AddToRoleAsync(defaultUser, ApplicationRoles.User);
        }

        var adminUserName = "admin@email.com";
        if (await userManager.FindByNameAsync(adminUserName) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminUserName,
                FirstName = "Demo",
                LastName = "Admin",
            };
            await userManager.CreateAsync(adminUser, DefaultPassword);
            adminUser = await userManager.FindByNameAsync(adminUserName);
            if (adminUser != null)
                await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);
        }
    }
}

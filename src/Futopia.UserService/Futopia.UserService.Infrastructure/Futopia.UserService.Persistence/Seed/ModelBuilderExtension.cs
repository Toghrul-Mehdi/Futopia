using Futopia.UserService.Domain.Entities;
using Futopia.UserService.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace Futopia.UserService.Persistence.Seed;
public static class ModelBuilderExtensions
{
    public static async Task UseUserSeedAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!roleManager.Roles.Any())
        {
            foreach (Roles role in Enum.GetValues(typeof(Roles)))
            {
                await roleManager.CreateAsync(new IdentityRole(role.ToString()));
            }
        }

        if (!userManager.Users.Any(x => x.NormalizedUserName == "SUPERADMIN"))
        {
            var superadmin = new AppUser
            {
                Id = Futopia.UserService.Domain.Helpers.UserIdGenerator.GenerateId(),
                FullName="Super Admin",
                UserName = "SuperAdmin",
                Email = "superadmin@gmail.com"
            };
            await userManager.CreateAsync(superadmin, "SuperAdmin123.");
            await userManager.AddToRoleAsync(superadmin, nameof(Roles.SuperAdmin));
        }
    }
}

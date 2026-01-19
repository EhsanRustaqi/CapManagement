using CapManagement.Server.AppicationUserModels;
using Microsoft.AspNetCore.Identity;

namespace CapManagement.Server.DbContexts.Seed
{
    public static class RoleSeeder
    {

        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = { "Owner", "CompanyAdmin", "Driver" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper(),
                        IsSystemRole = true
                    });
                }
            }

        }
    }
}

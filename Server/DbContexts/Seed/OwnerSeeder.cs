using CapManagement.Server.AppicationUserModels;
using Microsoft.AspNetCore.Identity;

namespace CapManagement.Server.DbContexts.Seed
{
    public static class OwnerSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            var email = "Ehsanaman50@gmail.com";

            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return;


            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, "Owner@123");
            await userManager.AddToRoleAsync(user, "Owner");


        }


    }
}

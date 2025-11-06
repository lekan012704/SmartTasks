using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartTask.Application.Dto.Account;
using SmartTask.Application.Enums;
using SmartTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Identity.Seeds
{
    public static class DefaultSuperAdmin
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string superAdminEmail = "superadmin@smarttask.com";
            string password = "SuperAdmin@123";
            string role = "SuperAdmin";

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await userManager.FindByEmailAsync(superAdminEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail, 
                    EmailConfirmed = true,
                    CreatedBy = superAdminEmail,
                    CompanyName = "SmartTask",
                    CompanyId = null,
                    IsActive = true,
                    DateCreated = DateTime.UtcNow
                };


                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }

}

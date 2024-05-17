using AppCacheAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppCacheAPI.Services
{
    public class GoogleAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GoogleAuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        public async Task CreateOrGetUser(ClaimsPrincipal principal)
        {
            var name = principal.FindFirstValue(ClaimTypes.Name);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = name,
                        Email = email,
                        IsGoogleUser = true
                    };
                    var result = await userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Failed to create user");
                    }

                    const string userRole = "User";
                    if (!await roleManager.RoleExistsAsync(userRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(userRole));
                    }

                    await userManager.AddToRoleAsync(user, userRole);
                }
                await signInManager.SignInAsync(user, isPersistent: false);
            }
        }
    }   
}
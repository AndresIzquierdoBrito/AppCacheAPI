using AppCacheAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppCacheAPI.Services
{
    public class GoogleAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public GoogleAuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public async Task CreateOrGetUser(ClaimsPrincipal principal)
        {
            var name = principal.FindFirstValue(ClaimTypes.Name);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        IsGoogleUser = true
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        // Handle the error
                        throw new Exception("Failed to create user");
                    }
                }

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
            }
        }
    }   
}
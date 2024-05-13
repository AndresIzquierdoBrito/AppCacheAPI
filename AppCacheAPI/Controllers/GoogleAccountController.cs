using AppCacheAPI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppCacheAPI.Services;

namespace AppCacheAPI.Controllers
{
    [Route("/")]
    [ApiController]
    public class GoogleAccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GoogleAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public GoogleAccountController(UserManager<ApplicationUser> userManager, GoogleAuthService authService, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _authService = authService;
            _signInManager = signInManager;
        }


        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var props = new AuthenticationProperties { RedirectUri = "/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleLogin()
        {
            var response = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (response.Principal == null) return BadRequest();

            await _authService.CreateOrGetUser(response.Principal);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(response.Principal));

            return Ok();
        }
    }
}

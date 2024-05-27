using AppCacheAPI.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppCacheAPI.Services;
using Microsoft.EntityFrameworkCore;
using AppCacheAPI.Data;

namespace AppCacheAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GoogleAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppCacheDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            GoogleAuthService authService, 
            RoleManager<IdentityRole> roleManager,
            AppCacheDbContext context)
        {
            _userManager = userManager;
            _authService = authService;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCredentials model)
        {
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);
            
            const string userRole = "User";
            if (!await _roleManager.RoleExistsAsync(userRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(userRole));
            }
            var category = new Category
            {
                Title = "ALLIDEAS",
                UserId = user.Id
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, userRole);

            return Ok();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCredentials model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);

            var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? string.Empty));

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity), authProperties);

            return Ok();

        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Ok();
        }

        [HttpGet("isAuthenticated")]
        public IActionResult IsAuthenticated()
        {
            return Ok(User.Identity is { IsAuthenticated: true });
        }


        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var props = new AuthenticationProperties { RedirectUri = "https://appcache.izbri.com/api/account/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleLogin()
        {
            var response = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (response.Principal == null) return BadRequest();

            await _authService.CreateOrGetUser(response.Principal);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(response.Principal), authProperties);

            return Redirect("http://localhost:5173/login");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
    }
}

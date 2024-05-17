using AppCacheAPI.Data;
using AppCacheAPI.Models;
using AppCacheAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;


services.AddControllers();

services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var connectionString = configuration.GetConnectionString("DefaultConnection");

services.AddDbContext<AppCacheDbContext>(options =>
    options.UseNpgsql(connectionString));

services.AddAuthorization();

services.AddIdentityApiEndpoints<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppCacheDbContext>();

services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddCookie()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException();
        googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException();
        googleOptions.Events.OnTicketReceived = async context =>
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<GoogleAuthService>();
            await authService.CreateOrGetUser(context.Principal ?? throw new InvalidOperationException());
        };
    });

services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedAccount = false;
    options.ClaimsIdentity.RoleClaimType = "User";
    options.User.RequireUniqueEmail = true;
    //options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});
services.ConfigureExternalCookie(options =>
{
    options.Cookie.Name = "ExternalAuthCookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None; // Same site shouldn't be none in production
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/";
    options.Cookie.SameSite = SameSiteMode.None; // Same site shouldn't be none in production
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services.AddEndpointsApiExplorer();

services.AddScoped<GoogleAuthService>();

services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "AppCache API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppCacheDbContext>();
//    dbContext.Database.Migrate();
//}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapIdentityApi<ApplicationUser>();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

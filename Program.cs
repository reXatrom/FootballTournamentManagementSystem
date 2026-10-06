using FootballTournamentManagementSystem.Components;
using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using FootballTournamentManagementSystem.Services;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=FootballTournamentManagementSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ICurrentUserDbContextFactory, CurrentUserDbContextFactory>();
builder.Services.AddScoped<IRecordAccessService, RecordAccessService>();
builder.Services.AddScoped<MatchEventValidationService>();
builder.Services.AddScoped<PlayerStatsService>();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    var existingSigningIn = options.Events.OnSigningIn;
    options.Events.OnSigningIn = async context =>
    {
        if (existingSigningIn is not null)
        {
            await existingSigningIn(context);
        }

        var identity = context.Principal?.Identities.FirstOrDefault(item => item.IsAuthenticated);
        if (identity is null)
        {
            return;
        }

        const string expiryClaim = "ftms:session-expires";
        foreach (var existingClaim in identity.FindAll(expiryClaim).ToList())
        {
            identity.RemoveClaim(existingClaim);
        }

        var expiresAt = context.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.Add(options.ExpireTimeSpan);
        identity.AddClaim(new Claim(expiryClaim, expiresAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));
    };
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider>();

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "Administrator", "Tournament Manager", "Player" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    foreach (var existingAdmin in await userManager.GetUsersInRoleAsync("Administrator"))
    {
        if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
        {
            await userManager.AddToRoleAsync(existingAdmin, "Admin");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        var adminEmail = app.Configuration["DevelopmentAdmin:Email"];
        var adminPassword = app.Configuration["DevelopmentAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = app.Configuration["DevelopmentAdmin:FullName"] ?? "Development Admin",
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        await DevelopmentDataSeeder.SeedIfEmptyAsync(scope.ServiceProvider);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorPages();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

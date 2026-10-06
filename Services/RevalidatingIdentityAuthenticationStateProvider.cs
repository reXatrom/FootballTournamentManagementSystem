using System.Security.Claims;
using FootballTournamentManagementSystem.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FootballTournamentManagementSystem.Services;

public sealed class RevalidatingIdentityAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> identityOptions)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        var expiryClaim = authenticationState.User.FindFirstValue("ftms:session-expires");
        if (long.TryParse(expiryClaim, out var expirySeconds)
            && DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(expirySeconds))
        {
            return false;
        }

        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(authenticationState.User);
        if (user is null)
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var claimType = identityOptions.Value.ClaimsIdentity.SecurityStampClaimType;
        var principalStamp = authenticationState.User.FindFirstValue(claimType);
        var currentStamp = await userManager.GetSecurityStampAsync(user);
        return string.Equals(principalStamp, currentStamp, StringComparison.Ordinal);
    }
}
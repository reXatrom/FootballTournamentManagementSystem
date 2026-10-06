using System.Security.Claims;
using FootballTournamentManagementSystem.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public sealed class CurrentUserDbContextFactory(
    IDbContextFactory<ApplicationDbContext> factory,
    AuthenticationStateProvider authenticationStateProvider) : ICurrentUserDbContextFactory
{
    public async Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var context = await factory.CreateDbContextAsync(cancellationToken);
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        context.CurrentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        context.IsAdmin = user.IsInRole("Admin");
        return context;
    }
}
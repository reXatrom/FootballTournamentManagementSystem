using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public enum RecordAccessDecision
{
    NotFound,
    Forbidden,
    Allowed
}

public interface IRecordAccessService
{
    Task<RecordAccessDecision> CheckAsync<TEntity>(int id, CancellationToken cancellationToken = default)
        where TEntity : class, IOwnedRecord;

    Task<RecordAccessDecision> CheckMatchEventAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class RecordAccessService(
    IDbContextFactory<ApplicationDbContext> factory,
    AuthenticationStateProvider authenticationStateProvider) : IRecordAccessService
{
    public async Task<RecordAccessDecision> CheckAsync<TEntity>(int id, CancellationToken cancellationToken = default)
        where TEntity : class, IOwnedRecord
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RecordAccessDecision.Forbidden;
        }

        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var ownerId = await db.Set<TEntity>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(record => record.Id == id)
            .Select(record => record.CreatedByUserId)
            .SingleOrDefaultAsync(cancellationToken);

        if (ownerId is null)
        {
            return RecordAccessDecision.NotFound;
        }

        return user.IsInRole("Admin") || ownerId == userId
            ? RecordAccessDecision.Allowed
            : RecordAccessDecision.Forbidden;
    }

    public async Task<RecordAccessDecision> CheckMatchEventAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RecordAccessDecision.Forbidden;
        }

        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var owners = await db.MatchEvents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(matchEvent => matchEvent.Id == id)
            .Select(matchEvent => new
            {
                EventOwnerId = matchEvent.CreatedByUserId,
                MatchOwnerId = matchEvent.Match.CreatedByUserId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (owners is null)
        {
            return RecordAccessDecision.NotFound;
        }

        return user.IsInRole("Admin") || owners.EventOwnerId == userId && owners.MatchOwnerId == userId
            ? RecordAccessDecision.Allowed
            : RecordAccessDecision.Forbidden;
    }
}
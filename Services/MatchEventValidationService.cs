using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public sealed class MatchEventValidationService(ICurrentUserDbContextFactory dbFactory)
{
    public async Task<string?> ValidateAsync(
        int matchId,
        MatchEventType eventType,
        int scorerId,
        int? assisterId,
        int? minute,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var matchTeams = await db.Matches.AsNoTracking()
            .Where(match => match.Id == matchId)
            .Select(match => new { match.HomeTeamId, match.AwayTeamId })
            .SingleOrDefaultAsync(cancellationToken);
        if (matchTeams is null)
        {
            return "The selected match is no longer available.";
        }

        var playerIds = assisterId.HasValue
            ? new[] { scorerId, assisterId.Value }.Distinct().ToArray()
            : new[] { scorerId };
        var playerTeams = await db.Players.AsNoTracking()
            .Where(player => playerIds.Contains(player.Id))
            .Select(player => new { player.Id, player.TeamId })
            .ToDictionaryAsync(player => player.Id, player => player.TeamId, cancellationToken);

        if (!playerTeams.TryGetValue(scorerId, out var scorerTeamId))
        {
            return "Select a scorer from one of the teams in this match.";
        }

        int? assisterTeamId = null;
        if (assisterId.HasValue)
        {
            if (!playerTeams.TryGetValue(assisterId.Value, out var foundAssisterTeamId))
            {
                return "Select an assister from one of the teams in this match.";
            }

            assisterTeamId = foundAssisterTeamId;
        }

        return ValidateParticipants(
            matchTeams.HomeTeamId,
            matchTeams.AwayTeamId,
            eventType,
            scorerId,
            scorerTeamId,
            assisterId,
            assisterTeamId,
            minute);
    }

    public static string? ValidateParticipants(
        int homeTeamId,
        int awayTeamId,
        MatchEventType eventType,
        int scorerId,
        int scorerTeamId,
        int? assisterId,
        int? assisterTeamId,
        int? minute)
    {
        if (!Enum.IsDefined(eventType))
        {
            return "Select a valid match event type.";
        }

        if (minute.HasValue && minute.Value is < 0 or > 130)
        {
            return "The event minute must be between 0 and 130.";
        }

        if (scorerTeamId != homeTeamId && scorerTeamId != awayTeamId)
        {
            return "The scorer must belong to one of the teams in this match.";
        }

        if (!assisterId.HasValue)
        {
            return null;
        }

        if (eventType == MatchEventType.OwnGoal)
        {
            return "An own goal cannot have an assist.";
        }

        if (assisterId.Value == scorerId)
        {
            return "The scorer cannot assist their own goal.";
        }

        if (assisterTeamId != scorerTeamId)
        {
            return "The assist provider must be on the scorer's team.";
        }

        return null;
    }
}

using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public sealed record MatchScore(int HomeScore, int AwayScore, string? ValidationError = null);

public static class MatchScoreCalculator
{
    public static async Task<Dictionary<int, MatchScore>> CalculateAsync(
        ApplicationDbContext db,
        IEnumerable<int> matchIds,
        bool includeAllOwners = false,
        IReadOnlyDictionary<int, (int HomeTeamId, int AwayTeamId)>? teamOverrides = null,
        CancellationToken cancellationToken = default)
    {
        var ids = matchIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<int, MatchScore>();
        }

        IQueryable<Match> matchesQuery = db.Matches.AsNoTracking().Where(match => ids.Contains(match.Id));
        IQueryable<MatchEvent> eventsQuery = db.MatchEvents.AsNoTracking()
            .Where(matchEvent => ids.Contains(matchEvent.MatchId)
                && (matchEvent.EventType == MatchEventType.Goal
                    || matchEvent.EventType == MatchEventType.PenaltyGoal
                    || matchEvent.EventType == MatchEventType.OwnGoal));

        if (includeAllOwners)
        {
            matchesQuery = matchesQuery.IgnoreQueryFilters();
            eventsQuery = eventsQuery.IgnoreQueryFilters();
        }

        var matchTeams = await matchesQuery
            .Select(match => new { match.Id, match.HomeTeamId, match.AwayTeamId })
            .ToListAsync(cancellationToken);
        var result = matchTeams.ToDictionary(match => match.Id, _ => new MatchScore(0, 0));
        if (matchTeams.Count == 0)
        {
            return result;
        }

        var eventCounts = await eventsQuery
            .GroupBy(matchEvent => new
            {
                matchEvent.MatchId,
                ScorerTeamId = matchEvent.GoalScorer.TeamId,
                matchEvent.EventType
            })
            .Select(group => new
            {
                group.Key.MatchId,
                group.Key.ScorerTeamId,
                group.Key.EventType,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        var teamIdsByMatch = matchTeams.ToDictionary(
            match => match.Id,
            match => teamOverrides is not null && teamOverrides.TryGetValue(match.Id, out var teamIds)
                ? teamIds
                : (match.HomeTeamId, match.AwayTeamId));
        foreach (var count in eventCounts)
        {
            if (!teamIdsByMatch.TryGetValue(count.MatchId, out var teamIds))
            {
                continue;
            }

            if (count.ScorerTeamId != teamIds.HomeTeamId && count.ScorerTeamId != teamIds.AwayTeamId)
            {
                result[count.MatchId] = result[count.MatchId] with
                {
                    ValidationError = "A goal event references a player outside the match teams."
                };
                continue;
            }

            var scoresHomeTeam = count.EventType == MatchEventType.OwnGoal
                ? count.ScorerTeamId == teamIds.AwayTeamId
                : count.ScorerTeamId == teamIds.HomeTeamId;
            var current = result[count.MatchId];
            result[count.MatchId] = scoresHomeTeam
                ? current with { HomeScore = current.HomeScore + count.Count }
                : current with { AwayScore = current.AwayScore + count.Count };
        }

        return result;
    }
}

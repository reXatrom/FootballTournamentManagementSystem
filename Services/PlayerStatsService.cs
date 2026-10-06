using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public sealed record PlayerStatsDto(
    int PlayerId,
    string PlayerName,
    int TeamId,
    string TeamName,
    int TournamentId,
    string TournamentName,
    int MatchesPlayed,
    int Goals,
    int PenaltyGoals,
    int Assists,
    int OwnGoals)
{
    public int GoalContributions => Goals + Assists;
}

public sealed record PlayerMatchContributionDto(
    int MatchId,
    DateTime MatchDate,
    string Fixture,
    int? Minute,
    string Contribution);

public sealed class PlayerStatsService(ICurrentUserDbContextFactory dbFactory)
{
    public async Task<PlayerStatsDto?> GetPlayerStatsAsync(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var allStats = await GetAllPlayerStatsAsync(cancellationToken: cancellationToken);
        return allStats.SingleOrDefault(stats => stats.PlayerId == playerId);
    }

    public async Task<IReadOnlyList<PlayerStatsDto>> GetAllPlayerStatsAsync(
        int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var playersQuery = db.Players.IgnoreQueryFilters().AsNoTracking();
        if (tournamentId.HasValue)
        {
            playersQuery = playersQuery.Where(player => player.Team.TournamentId == tournamentId.Value);
        }

        var players = await playersQuery
            .OrderBy(player => player.Team.Name)
            .ThenBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .Select(player => new PlayerIdentity(
                player.Id,
                player.FirstName + " " + player.LastName,
                player.TeamId,
                player.Team.Name,
                player.Team.TournamentId,
                player.Team.Tournament.Name))
            .ToListAsync(cancellationToken);

        var goalEvents = GetEndedGoalEvents(db, tournamentId);
        var goalCounts = await goalEvents
            .GroupBy(matchEvent => matchEvent.GoalScorerId)
            .Select(group => new
            {
                PlayerId = group.Key,
                Goals = group.Count(matchEvent => matchEvent.EventType == MatchEventType.Goal
                    || matchEvent.EventType == MatchEventType.PenaltyGoal),
                PenaltyGoals = group.Count(matchEvent => matchEvent.EventType == MatchEventType.PenaltyGoal),
                OwnGoals = group.Count(matchEvent => matchEvent.EventType == MatchEventType.OwnGoal)
            })
            .ToDictionaryAsync(row => row.PlayerId, cancellationToken);

        var assists = await goalEvents
            .Where(matchEvent => matchEvent.EventType != MatchEventType.OwnGoal
                && matchEvent.AssistedByPlayerId.HasValue)
            .GroupBy(matchEvent => matchEvent.AssistedByPlayerId!.Value)
            .Select(group => new { PlayerId = group.Key, Assists = group.Count() })
            .ToDictionaryAsync(row => row.PlayerId, row => row.Assists, cancellationToken);

        var scorerAppearances = goalEvents
            .Select(matchEvent => new { PlayerId = matchEvent.GoalScorerId, matchEvent.MatchId });
        var assisterAppearances = goalEvents
            .Where(matchEvent => matchEvent.EventType != MatchEventType.OwnGoal
                && matchEvent.AssistedByPlayerId.HasValue)
            .Select(matchEvent => new { PlayerId = matchEvent.AssistedByPlayerId!.Value, matchEvent.MatchId });
        var matchesWithEvents = await scorerAppearances
            .Concat(assisterAppearances)
            .GroupBy(item => item.PlayerId)
            .Select(group => new
            {
                PlayerId = group.Key,
                MatchesPlayed = group.Select(item => item.MatchId).Distinct().Count()
            })
            .ToDictionaryAsync(row => row.PlayerId, row => row.MatchesPlayed, cancellationToken);

        return players.Select(player =>
        {
            goalCounts.TryGetValue(player.PlayerId, out var goals);
            assists.TryGetValue(player.PlayerId, out var assistCount);
            matchesWithEvents.TryGetValue(player.PlayerId, out var matchesPlayed);
            return new PlayerStatsDto(
                player.PlayerId,
                player.PlayerName,
                player.TeamId,
                player.TeamName,
                player.TournamentId,
                player.TournamentName,
                matchesPlayed,
                goals?.Goals ?? 0,
                goals?.PenaltyGoals ?? 0,
                assistCount,
                goals?.OwnGoals ?? 0);
        }).ToList();
    }

    public async Task<IReadOnlyList<PlayerStatsDto>> GetTopScorersAsync(
        int count,
        int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return Array.Empty<PlayerStatsDto>();
        }

        var ordered = (await GetAllPlayerStatsAsync(tournamentId, cancellationToken))
            .OrderByDescending(player => player.Goals)
            .ThenByDescending(player => player.Assists)
            .ThenBy(player => player.PlayerName)
            .ToList();
        if (ordered.Count == 0 || ordered[0].Goals == 0)
        {
            return Array.Empty<PlayerStatsDto>();
        }

        return IncludeJointLeaders(ordered, count, player => player.Goals);
    }

    public async Task<IReadOnlyList<PlayerStatsDto>> GetTopAssistProvidersAsync(
        int count,
        int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return Array.Empty<PlayerStatsDto>();
        }

        var ordered = (await GetAllPlayerStatsAsync(tournamentId, cancellationToken))
            .OrderByDescending(player => player.Assists)
            .ThenByDescending(player => player.Goals)
            .ThenBy(player => player.PlayerName)
            .ToList();
        if (ordered.Count == 0 || ordered[0].Assists == 0)
        {
            return Array.Empty<PlayerStatsDto>();
        }

        return IncludeJointLeaders(ordered, count, player => player.Assists);
    }

    public async Task<IReadOnlyList<PlayerMatchContributionDto>> GetPlayerMatchContributionsAsync(
        int playerId,
        int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var events = GetEndedGoalEvents(db, tournamentId)
            .Where(matchEvent => matchEvent.GoalScorerId == playerId
                || matchEvent.AssistedByPlayerId == playerId);
        var rows = await events
            .OrderBy(matchEvent => matchEvent.Match.MatchDate)
            .ThenBy(matchEvent => matchEvent.Minute)
            .Select(matchEvent => new
            {
                matchEvent.MatchId,
                matchEvent.Match.MatchDate,
                HomeTeam = matchEvent.Match.HomeTeam.Name,
                AwayTeam = matchEvent.Match.AwayTeam.Name,
                matchEvent.Minute,
                matchEvent.EventType,
                matchEvent.GoalScorerId,
                matchEvent.AssistedByPlayerId
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new PlayerMatchContributionDto(
            row.MatchId,
            row.MatchDate,
            $"{row.HomeTeam} vs {row.AwayTeam}",
            row.Minute,
            row.GoalScorerId == playerId
                ? row.EventType switch
                {
                    MatchEventType.OwnGoal => "Own goal",
                    MatchEventType.PenaltyGoal => "Penalty goal",
                    _ => "Goal"
                }
                : "Assist"))
            .ToList();
    }

    private static IQueryable<MatchEvent> GetEndedGoalEvents(ApplicationDbContext db, int? tournamentId)
    {
        var events = db.MatchEvents.IgnoreQueryFilters().AsNoTracking()
            .Where(matchEvent => matchEvent.Match.Status == MatchStatus.Ended
                && (matchEvent.EventType == MatchEventType.Goal
                    || matchEvent.EventType == MatchEventType.PenaltyGoal
                    || matchEvent.EventType == MatchEventType.OwnGoal));

        return tournamentId.HasValue
            ? events.Where(matchEvent => matchEvent.Match.TournamentId == tournamentId.Value)
            : events;
    }

    private static IReadOnlyList<PlayerStatsDto> IncludeJointLeaders(
        IReadOnlyList<PlayerStatsDto> ordered,
        int count,
        Func<PlayerStatsDto, int> leaderValue)
    {
        if (ordered.Count <= count)
        {
            return ordered;
        }

        var leaderCount = ordered.TakeWhile(player => leaderValue(player) == leaderValue(ordered[0])).Count();
        return ordered.Take(Math.Max(count, leaderCount)).ToList();
    }

    private sealed record PlayerIdentity(
        int PlayerId,
        string PlayerName,
        int TeamId,
        string TeamName,
        int TournamentId,
        string TournamentName);
}

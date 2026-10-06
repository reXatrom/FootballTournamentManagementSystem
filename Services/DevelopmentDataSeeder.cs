using FootballTournamentManagementSystem.Data;
using FootballTournamentManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Services;

public static class DevelopmentDataSeeder
{
    public static async Task SeedIfEmptyAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DevelopmentDataSeeder));
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var owner = await userManager.Users.OrderBy(user => user.Id).FirstOrDefaultAsync();
        if (owner is null)
        {
            logger.LogInformation("Skipping development sample data because no user account exists.");
            return;
        }

        var factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Tournaments.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        db.CurrentUserId = owner.Id;
        db.IsAdmin = await userManager.IsInRoleAsync(owner, "Admin");
        var today = DateTime.UtcNow.Date;
        var tournament = new Tournament
        {
            Name = "Development Cup",
            Description = "Sample tournament data for local development.",
            StartDate = today.AddDays(-7),
            EndDate = today.AddDays(21),
            Location = "Development Ground",
            Status = TournamentStatus.Ongoing
        };
        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        var homeTeam = new Team { Name = "Harbour FC", TournamentId = tournament.Id };
        var awayTeam = new Team { Name = "Valley United", TournamentId = tournament.Id };
        db.Teams.AddRange(homeTeam, awayTeam);
        await db.SaveChangesAsync();

        var homePlayers = new[]
        {
            new Player { FirstName = "Alex", LastName = "Morgan", Position = PlayerPosition.Forward, JerseyNumber = 9, TeamId = homeTeam.Id },
            new Player { FirstName = "Jamie", LastName = "Lee", Position = PlayerPosition.Midfielder, JerseyNumber = 8, TeamId = homeTeam.Id },
            new Player { FirstName = "Casey", LastName = "Rivera", Position = PlayerPosition.Defender, JerseyNumber = 4, TeamId = homeTeam.Id }
        };
        var awayPlayers = new[]
        {
            new Player { FirstName = "Taylor", LastName = "Reed", Position = PlayerPosition.Forward, JerseyNumber = 10, TeamId = awayTeam.Id },
            new Player { FirstName = "Jordan", LastName = "Kim", Position = PlayerPosition.Midfielder, JerseyNumber = 6, TeamId = awayTeam.Id },
            new Player { FirstName = "Riley", LastName = "Singh", Position = PlayerPosition.Goalkeeper, JerseyNumber = 1, TeamId = awayTeam.Id }
        };
        db.Players.AddRange(homePlayers.Concat(awayPlayers));
        await db.SaveChangesAsync();

        var endedMatch = new Match
        {
            TournamentId = tournament.Id,
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            MatchDate = today.AddDays(-1),
            Location = "Harbour Stadium",
            Status = MatchStatus.Ended
        };
        var scheduledMatch = new Match
        {
            TournamentId = tournament.Id,
            HomeTeamId = awayTeam.Id,
            AwayTeamId = homeTeam.Id,
            MatchDate = today.AddDays(3),
            Location = "Valley Park",
            Status = MatchStatus.Scheduled
        };
        db.Matches.AddRange(endedMatch, scheduledMatch);
        await db.SaveChangesAsync();

        db.MatchEvents.AddRange(
            new MatchEvent
            {
                MatchId = endedMatch.Id,
                GoalScorerId = homePlayers[0].Id,
                AssistedByPlayerId = homePlayers[1].Id,
                EventType = MatchEventType.Goal,
                Minute = 12
            },
            new MatchEvent
            {
                MatchId = endedMatch.Id,
                GoalScorerId = awayPlayers[0].Id,
                AssistedByPlayerId = awayPlayers[1].Id,
                EventType = MatchEventType.PenaltyGoal,
                Minute = 38
            },
            new MatchEvent
            {
                MatchId = endedMatch.Id,
                GoalScorerId = homePlayers[2].Id,
                EventType = MatchEventType.OwnGoal,
                Minute = 71
            },
            new MatchEvent
            {
                MatchId = endedMatch.Id,
                GoalScorerId = homePlayers[0].Id,
                AssistedByPlayerId = homePlayers[2].Id,
                EventType = MatchEventType.Goal,
                Minute = 84
            });
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded development tournament, teams, players, matches, and goal events.");
    }
}

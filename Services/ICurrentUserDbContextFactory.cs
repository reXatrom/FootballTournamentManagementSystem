using FootballTournamentManagementSystem.Data;

namespace FootballTournamentManagementSystem.Services;

public interface ICurrentUserDbContextFactory
{
    Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}
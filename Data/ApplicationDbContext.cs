using FootballTournamentManagementSystem.Models;
using FootballTournamentManagementSystem.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FootballTournamentManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    public string? CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.Location).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(150);
            entity.Property(t => t.LogoUrl).HasMaxLength(500);

            entity.HasOne(t => t.Tournament)
                .WithMany(t => t.Teams)
                .HasForeignKey(t => t.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Manager)
                .WithMany()
                .HasForeignKey(t => t.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => new { t.TournamentId, t.Name })
                .IsUnique();
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Position).HasConversion<string>();
            entity.Property(p => p.JerseyNumber).IsRequired();

            entity.HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => new { p.TeamId, p.JerseyNumber }).IsUnique();
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Location).HasMaxLength(200);
            entity.Property(m => m.Status).HasConversion<string>();

            entity.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.HomeTeam)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.AwayTeam)
                .WithMany(t => t.AwayMatches)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => new { m.TournamentId, m.HomeTeamId, m.AwayTeamId, m.MatchDate });
        });

        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.HasKey(me => me.Id);
            entity.Property(me => me.EventType).HasConversion<string>();
            entity.Property(me => me.Minute).IsRequired(false);

            entity.HasOne(me => me.Match)
                .WithMany(m => m.MatchEvents)
                .HasForeignKey(me => me.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(me => me.GoalScorer)
                .WithMany(p => p.GoalsScored)
                .HasForeignKey(me => me.GoalScorerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(me => me.AssistedByPlayer)
                .WithMany(p => p.AssistsProvided)
                .HasForeignKey(me => me.AssistedByPlayerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        ConfigureOwnership<Tournament>(modelBuilder);
        ConfigureOwnership<Team>(modelBuilder);
        ConfigureOwnership<Player>(modelBuilder);
        ConfigureOwnership<Match>(modelBuilder);
        ConfigureOwnership<MatchEvent>(modelBuilder);

        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.Property(activity => activity.UserId).IsRequired().HasMaxLength(450);
            entity.Property(activity => activity.Action).IsRequired().HasMaxLength(40);
            entity.Property(activity => activity.EntityType).IsRequired().HasMaxLength(80);
            entity.Property(activity => activity.Description).IsRequired().HasMaxLength(500);
            entity.Property(activity => activity.Route).HasMaxLength(1000);
            entity.Property(activity => activity.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(activity => activity.User)
                .WithMany()
                .HasForeignKey(activity => activity.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(activity => new { activity.UserId, activity.CreatedAt });
            entity.HasQueryFilter(activity => IsAdmin || activity.UserId == CurrentUserId);
        });
    }

    private void ConfigureOwnership<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IOwnedRecord
    {
        modelBuilder.Entity<TEntity>(entity =>
        {
            entity.Property(record => record.CreatedByUserId).IsRequired();
            entity.Property(record => record.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(record => record.RowVersion).IsRowVersion();
            entity.HasOne(record => record.CreatedByUser)
                .WithMany()
                .HasForeignKey(record => record.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(record => record.UpdatedByUser)
                .WithMany()
                .HasForeignKey(record => record.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(record => IsAdmin || record.CreatedByUserId == CurrentUserId);
        });
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => SaveChangesWithMatchScoresAsync(acceptAllChangesOnSuccess, CancellationToken.None).GetAwaiter().GetResult();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        return await SaveChangesWithMatchScoresAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task<int> SaveChangesWithMatchScoresAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken)
    {
        ChangeTracker.DetectChanges();
        var affectedMatchIds = ChangeTracker.Entries<MatchEvent>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .SelectMany(entry => entry.State == EntityState.Added
                ? new[] { entry.Entity.MatchId }
                : new[] { entry.Entity.MatchId, (int)entry.Property(item => item.MatchId).OriginalValue! })
            .Concat(ChangeTracker.Entries<Match>()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                .Select(entry => entry.Entity.Id))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        await using var transaction = affectedMatchIds.Length > 0
            && Database.IsRelational()
            && Database.CurrentTransaction is null
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;

        try
        {
            await PrepareOwnershipChangesAsync(cancellationToken);
            var rowsWritten = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            if (affectedMatchIds.Length > 0)
            {
                var scores = await MatchScoreCalculator.CalculateAsync(
                    this,
                    affectedMatchIds,
                    includeAllOwners: true,
                    cancellationToken: cancellationToken);

                foreach (var (matchId, score) in scores)
                {
                    if (score.ValidationError is not null)
                    {
                        throw new InvalidOperationException(score.ValidationError);
                    }

                    var match = await Matches.IgnoreQueryFilters()
                        .SingleOrDefaultAsync(item => item.Id == matchId, cancellationToken);
                    if (match is not null && (match.HomeScore != score.HomeScore || match.AwayScore != score.AwayScore))
                    {
                        match.HomeScore = score.HomeScore;
                        match.AwayScore = score.AwayScore;
                        match.UpdatedByUserId = CurrentUserId;
                        match.UpdatedAt = DateTime.UtcNow;
                    }
                }

                if (ChangeTracker.HasChanges())
                {
                    rowsWritten += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return rowsWritten;
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }

    private async Task PrepareOwnershipChangesAsync(CancellationToken cancellationToken)
    {
        var changedRecords = ChangeTracker.Entries<IOwnedRecord>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        if (changedRecords.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentUserId))
        {
            throw new RecordAccessDeniedException("Sign in before changing tournament records.");
        }

        var now = DateTime.UtcNow;
        var activities = new List<UserActivity>(changedRecords.Count);
        foreach (var entry in changedRecords)
        {
            var record = entry.Entity;
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => string.Empty
            };
            var entityType = record.GetType().Name;
            activities.Add(new UserActivity
            {
                UserId = CurrentUserId,
                Action = action,
                EntityType = entityType,
                EntityId = record.Id > 0 ? record.Id : null,
                Description = $"{action} {entityType}",
                CreatedAt = now
            });

            if (entry.State == EntityState.Added)
            {
                record.CreatedByUserId = CurrentUserId;
                record.CreatedAt = now;
                record.UpdatedByUserId = null;
                record.UpdatedAt = null;
            }
            else
            {
                var originalOwnerId = (string?)entry.Property(item => item.CreatedByUserId).OriginalValue;
                if (!IsAdmin && !string.Equals(originalOwnerId, CurrentUserId, StringComparison.Ordinal))
                {
                    throw new RecordAccessDeniedException("You don't have permission to modify this record.");
                }

                entry.Property(item => item.CreatedByUserId).IsModified = false;
                entry.Property(item => item.CreatedAt).IsModified = false;
                if (entry.State == EntityState.Modified)
                {
                    record.UpdatedByUserId = CurrentUserId;
                    record.UpdatedAt = now;
                }
            }

            if (!IsAdmin)
            {
                var validDependencies = record switch
                {
                    Team team => await IsOwnedByCurrentUserAsync<Tournament>(team.TournamentId, cancellationToken).ConfigureAwait(false),
                    Player player => await IsOwnedByCurrentUserAsync<Team>(player.TeamId, cancellationToken).ConfigureAwait(false),
                    Match match => await IsOwnedByCurrentUserAsync<Tournament>(match.TournamentId, cancellationToken).ConfigureAwait(false)
                        && await IsOwnedByCurrentUserAsync<Team>(match.HomeTeamId, cancellationToken).ConfigureAwait(false)
                        && await IsOwnedByCurrentUserAsync<Team>(match.AwayTeamId, cancellationToken).ConfigureAwait(false),
                    _ => true
                };

                if (!validDependencies)
                {
                    throw new RecordAccessDeniedException("Related records must belong to your account.");
                }
            }

            if (record is MatchEvent matchEvent)
            {
                var match = await Matches.IgnoreQueryFilters()
                    .Where(item => item.Id == matchEvent.MatchId)
                    .Select(item => new { item.CreatedByUserId, item.HomeTeamId, item.AwayTeamId })
                    .SingleOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (match is null || !IsAdmin && !string.Equals(match.CreatedByUserId, CurrentUserId, StringComparison.Ordinal))
                {
                    throw new RecordAccessDeniedException("Only the match owner or an Admin may change its events.");
                }

                if (entry.State != EntityState.Deleted)
                {
                    var playerIds = new List<int> { matchEvent.GoalScorerId };
                    if (matchEvent.AssistedByPlayerId.HasValue)
                    {
                        playerIds.Add(matchEvent.AssistedByPlayerId.Value);
                    }

                    var playerTeams = await Players.IgnoreQueryFilters()
                        .Where(player => playerIds.Contains(player.Id))
                        .Select(player => new { player.Id, player.TeamId, player.CreatedByUserId })
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);
                    if (playerTeams.Count != playerIds.Distinct().Count()
                        || playerTeams.Any(player => player.TeamId != match.HomeTeamId && player.TeamId != match.AwayTeamId)
                        || !IsAdmin && playerTeams.Any(player => player.CreatedByUserId != CurrentUserId))
                    {
                        throw new RecordAccessDeniedException("Match event players must belong to one of the two teams.");
                    }

                    var scorerTeamId = playerTeams.Single(player => player.Id == matchEvent.GoalScorerId).TeamId;
                    int? assisterTeamId = matchEvent.AssistedByPlayerId.HasValue
                        ? playerTeams.Single(player => player.Id == matchEvent.AssistedByPlayerId.Value).TeamId
                        : null;
                    var validationError = MatchEventValidationService.ValidateParticipants(
                        match.HomeTeamId,
                        match.AwayTeamId,
                        matchEvent.EventType,
                        matchEvent.GoalScorerId,
                        scorerTeamId,
                        matchEvent.AssistedByPlayerId,
                        assisterTeamId,
                        matchEvent.Minute);
                    if (validationError is not null)
                    {
                        throw new InvalidOperationException(validationError);
                    }
                }
            }
        }

        UserActivities.AddRange(activities);
    }

    private Task<bool> IsOwnedByCurrentUserAsync<TEntity>(int id, CancellationToken cancellationToken)
        where TEntity : class, IOwnedRecord
    {
        return Set<TEntity>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(record => record.Id == id && record.CreatedByUserId == CurrentUserId, cancellationToken);
    }
}

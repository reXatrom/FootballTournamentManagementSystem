using FootballTournamentManagementSystem.Models;
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
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade);

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
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(me => me.GoalScorer)
                .WithMany(p => p.GoalsScored)
                .HasForeignKey(me => me.GoalScorerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(me => me.AssistedByPlayer)
                .WithMany(p => p.AssistsProvided)
                .HasForeignKey(me => me.AssistedByPlayerId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

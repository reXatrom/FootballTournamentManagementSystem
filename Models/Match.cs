using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public enum MatchStatus
{
    Scheduled = 0,
    Ended = 1,
    Cancelled = 2
}

public class Match : IOwnedRecord
{
    public int Id { get; set; }

    [Required]
    public int TournamentId { get; set; }

    [Required]
    public int HomeTeamId { get; set; }

    [Required]
    public int AwayTeamId { get; set; }

    [Required]
    public DateTime MatchDate { get; set; }

    public TimeOnly? MatchTime { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Range(0, int.MaxValue)]
    public int HomeScore { get; set; }

    [Range(0, int.MaxValue)]
    public int AwayScore { get; set; }

    [Required]
    public MatchStatus Status { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Tournament Tournament { get; set; } = null!;
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ApplicationUser? UpdatedByUser { get; set; }
    public ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();
}

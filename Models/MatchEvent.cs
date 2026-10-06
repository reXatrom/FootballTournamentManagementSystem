using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public enum MatchEventType
{
    Goal = 0,
    YellowCard = 1,
    RedCard = 2,
    Substitution = 3,
    OwnGoal = 4,
    PenaltyGoal = 5
}

public class MatchEvent : IOwnedRecord
{
    public int Id { get; set; }

    [Required]
    public int MatchId { get; set; }

    [Required]
    public int GoalScorerId { get; set; }

    public int? AssistedByPlayerId { get; set; }

    [Required]
    public MatchEventType EventType { get; set; }

    [Range(0, 130)]
    public int? Minute { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Match Match { get; set; } = null!;
    public Player GoalScorer { get; set; } = null!;
    public Player? AssistedByPlayer { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ApplicationUser? UpdatedByUser { get; set; }
}

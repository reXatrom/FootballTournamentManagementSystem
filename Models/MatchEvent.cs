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

public class MatchEvent
{
    public int Id { get; set; }

    [Required]
    public int MatchId { get; set; }

    [Required]
    public int GoalScorerId { get; set; }

    public int? AssistedByPlayerId { get; set; }

    [Required]
    public MatchEventType EventType { get; set; }

    [Range(1, 120)]
    public int? Minute { get; set; }

    public Match Match { get; set; } = null!;
    public Player GoalScorer { get; set; } = null!;
    public Player? AssistedByPlayer { get; set; }
}

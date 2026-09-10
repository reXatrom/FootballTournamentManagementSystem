using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public enum PlayerPosition
{
    Goalkeeper = 0,
    Defender = 1,
    Midfielder = 2,
    Forward = 3
}

public class Player
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public PlayerPosition Position { get; set; }

    [Range(1, 99)]
    public int JerseyNumber { get; set; }

    [Required]
    public int TeamId { get; set; }

    public string? UserId { get; set; }

    public Team Team { get; set; } = null!;
    public ApplicationUser? User { get; set; }

    public ICollection<MatchEvent> GoalsScored { get; set; } = new List<MatchEvent>();
    public ICollection<MatchEvent> AssistsProvided { get; set; } = new List<MatchEvent>();
}

using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public enum TournamentStatus
{
    Upcoming = 0,
    Ongoing = 1,
    Completed = 2
}

public class Tournament
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public TournamentStatus Status { get; set; }

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}

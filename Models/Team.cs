using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public class Team
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? ManagerId { get; set; }

    [Url]
    public string? LogoUrl { get; set; }

    [Required]
    public int TournamentId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Tournament Tournament { get; set; } = null!;
    public ApplicationUser? Manager { get; set; }
    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}

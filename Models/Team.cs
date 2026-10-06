using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public class Team : IOwnedRecord
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

    public string CreatedByUserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Tournament Tournament { get; set; } = null!;
    public ApplicationUser? Manager { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ApplicationUser? UpdatedByUser { get; set; }
    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}

using System.ComponentModel.DataAnnotations;

namespace FootballTournamentManagementSystem.Models;

public class UserActivity
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [Required, StringLength(40)]
    public string Action { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Route { get; set; }

    public DateTime CreatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
namespace FootballTournamentManagementSystem.Models;

public interface IOwnedRecord
{
    int Id { get; set; }
    string CreatedByUserId { get; set; }
    DateTime CreatedAt { get; set; }
    string? UpdatedByUserId { get; set; }
    DateTime? UpdatedAt { get; set; }
    byte[] RowVersion { get; set; }
    ApplicationUser CreatedByUser { get; set; }
    ApplicationUser? UpdatedByUser { get; set; }
}
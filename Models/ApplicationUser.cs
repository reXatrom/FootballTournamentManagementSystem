using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FootballTournamentManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
}

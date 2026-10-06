namespace FootballTournamentManagementSystem.Data;

public sealed class RecordAccessDeniedException(string message) : UnauthorizedAccessException(message);

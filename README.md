# Football Tournament Management System (FTMS)

This project is being built in phases as a beginner-friendly .NET Blazor application for managing football tournaments, teams, players, fixtures, results, statistics, and standings.

## Phase 1: Planning and Environment Setup

### Project Goal
Create a modern web application that helps tournament organizers organize fixtures, teams, players, and results in one place.

### Technology Choice
- ASP.NET Core
- Blazor
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Bootstrap
- Git and GitHub

### .NET Version
The project targets .NET 10.0; `global.json` pins the .NET 10 SDK feature band.

### Why Blazor?
Blazor is a good choice because it lets us build a full web application with C# instead of mixing JavaScript and C#. This reduces complexity and is ideal for a team project.

### Project Structure
The app is created under:
- `FootballTournamentManagementSystem/`

### Run the Application
From the project folder:

```bash
dotnet restore
dotnet build
dotnet run
```

Then open the local URL shown in the terminal.

### Current Status
The project includes tournament, roster, fixture, standings, Identity, ownership, and match-event workflows.

## Local development setup

Configure the development Admin credentials with user-secrets. The application seeds this user into the `Admin` role when both email and password are present; do not put these values in `appsettings.json` or source control.

```powershell
dotnet user-secrets set "DevelopmentAdmin:Email" "admin@example.test"
dotnet user-secrets set "DevelopmentAdmin:Password" "<choose-a-local-password>"
dotnet user-secrets set "DevelopmentAdmin:FullName" "Development Admin"
```

The background player reads its editable playlist from `wwwroot/audio/playlist.json`. Supply audio files you have rights to use at the configured paths: `uefa-champions-league-anthem.mp3`, `dai-dai.mp3`, `fire.mp3`, and `papaoutai.mp3`. No audio is bundled or downloaded by the application.

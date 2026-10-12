# Football Tournament Management System

A Blazor Server application for managing football tournaments, teams, players, match schedules, match events, and standings. The dashboard summarizes the competition and provides quick access to upcoming fixtures, recent results, and scoring leaders.

## Requirements

- Windows with .NET SDK `8.0.424` or a compatible .NET 8 feature-band SDK
- SQL Server LocalDB (the default connection) or another SQL Server instance
- EF Core command-line tools `8.0.12` for applying migrations

The repository's `global.json` selects the .NET SDK. Install the pinned SDK if `dotnet --version` reports that no compatible SDK is available.

## Run Locally

From the project directory:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef --version 8.0.12
dotnet ef database update
dotnet run
```

Open the URL printed by `dotnet run`. The HTTP launch profile uses `http://localhost:5135`.

The default database is `FootballTournamentManagementSystemDb` on SQL Server LocalDB. To use another SQL Server, store the connection string in .NET user secrets instead of committing credentials:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
```

## Main Workflows

1. Register or sign in from the account links.
2. Create a tournament with its dates, location, and description.
3. Add teams to the tournament, then register players to those teams.
4. Schedule matches between teams in the same tournament.
5. Record the final score and match events, then review match details and standings.

The dashboard shows tournament, team, player, and match totals; upcoming fixtures; recent results; and current goal and assist leaders. Standings and leaders depend on recorded match results and events.

## Areas

| Area | Routes | Purpose |
| --- | --- | --- |
| Dashboard | `/` | Competition summary, fixtures, results, quick actions, and leaders |
| Tournaments | `/tournaments` | Create, edit, and remove competitions |
| Teams | `/teams` | Manage tournament teams |
| Players | `/players` | Manage team rosters |
| Matches | `/matches` | Schedule fixtures, enter results, and view match details |
| Match events | `/match-events` | Record goals, cards, substitutions, and other match events |
| Standings | `/standings` | Review tournament tables and player leaders |
| Accounts | `/account/register`, `/account/login` | Register and sign in |

Creating and editing tournament data is restricted to the `Administrator` and `Tournament Manager` roles. The app creates these roles at startup. To promote an existing account during local development, set `DevelopmentAdminEmail` to that account's email before restarting the app; the account must already exist.

## Data and Validation

Entity Framework Core stores tournaments, teams, players, matches, match events, and Identity accounts in SQL Server. Database schema changes are tracked in `Migrations/` and can be applied with `dotnet ef database update`.

Forms use data-annotation validation and page-level checks for domain rules such as unique team names, unique jersey numbers within a team, valid tournament team pairings, and consistency between match scores and recorded goal events. The dashboard reports data-load failures with a retry action, and the router displays a recoverable fallback for unexpected page errors.

## Build

```powershell
dotnet build
```

The current project board and rubric-oriented follow-up work are tracked in [PROJECT_BOARD.md](PROJECT_BOARD.md).
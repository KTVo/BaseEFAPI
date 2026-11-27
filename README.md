# Base EF API

A minimal ASP.NET Core API scaffold targeting .NET 10, with Entity Framework Core organization models.

## Run locally

```sh
dotnet run --project BaseEFAPI.csproj
```

In the Development environment, Swagger is available at `/swagger`. There are currently no application controller endpoints.

## Organization data

`AgencyAdministrator` is the single persisted organization model. Each row captures agency and administrator details together as a historical snapshot. `NexusDbContext` maps it to the `AgencyAdministrator` table, and `NexusRepository` contains the snapshot persistence logic. `AgencyAdministratorResponse` describes the save outcome and is not a database entity.

`Program.cs` registers `NexusDbContext` and scoped `NexusRepository`. Set `Database:Provider` to `SqlServer` (default) or `PostgreSql`, and configure `ConnectionStrings:DefaultConnection` through local configuration or environment variables. The connection is required when database services are resolved. `appsettings.Example.json` shows the configuration shape; do not commit credentials.

The application does not create, upgrade, or delete database tables. Plan schema changes separately before enabling persistence.

## Build

```sh
dotnet build BaseEFAPI.slnx
```

## Using organization models

Inject `NexusRepository` into an import service or controller:

```csharp
AgencyAdministratorResponse outcome = await repository.AddAgencyAdministratorAsync(observation, cancellationToken);
AgencyAdministrator? current = await repository.GetCurrentAgencyAsync(cdsCode, cancellationToken);
List<AgencyAdministrator> history = await repository.GetAgencyHistoryAsync(cdsCode, cancellationToken);
```

`observation` is an `AgencyAdministrator` with a CDS code and administrator first/last names. The repository stamps `RecordedAtUtc` and appends when any agency or administrator detail differs from the latest observation. Identical details return `RecordHasChanged = false`. Comparisons are exact, including capitalization; the incoming CDS code is trimmed. Recording time and snapshot ID are excluded from the comparison.

`GetCurrentAgencyAsync` returns the latest untracked `AgencyAdministrator` snapshot for a CDS code, or null when no record exists. `GetAgencyHistoryAsync` returns untracked snapshots newest first, with ID as a deterministic timestamp tie-breaker. IDs identify observations, not independently managed agencies or people.

This supports one tracked administrator position per agency. Multiple simultaneous administrators require a position/source identifier. Concurrent imports for the same agency should be serialized by the caller; the check-and-insert operation does not prevent concurrent duplicate observations.

`RecordedAtUtc` maps to the existing `TimeStampUTC` database column. No database migration or automatic table creation is performed. The current-agency view represents the latest ingested observation, not independently verified employment dates. No HTTP endpoints are added by these repository methods.

## Nested snapshot details

`AgencyAdministrator` contains `Agency` and `Person` complex properties. Both objects are required, while their individual fields retain their nullable database types. They have no independent IDs or tables. Explicit EF column mappings preserve the existing flat `AgencyAdministrator` table, including `AgencyName`, `AgencyAddress`, `AgencyEmail`, `PersonEmail`, and `TimeStampUTC`.

```csharp
AgencyAdministrator observation = new AgencyAdministrator
{
    Agency = new Agency { CDSCode = "01234560000000", Name = "Example District" },
    Person = new Person { FirstName = "Jane", LastName = "Doe", Email = "jane@example.org" }
};
```

CSV importers must map flat columns into these nested properties (for example, `AgencyName` to `Agency.Name` and `PersonEmail` to `Person.Email`). No CSV importer is currently implemented. JSON serialization now produces nested `Agency` and `Person` objects. The repository still compares all captured details and returns complete snapshots for current and history queries. This restructuring does not automatically migrate or modify a database.

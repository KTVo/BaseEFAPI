<!-- PROJECT SHIELDS -->
[![Version][version-shield]][version-url]

<!-- PAGE TITLE -->
<div>
  <!-- COMPANY LOGO -->
  <h1  align="center"> Base EF (Entity Framework) API</h1>
</div>

<!-- DESCRIPTION -->
> Base EF API is a barebone C# ASP.NET Web API using .Net 10 that will connect to any SQL database. To begin visit the program.cs to identify how to pass the connection string from appsettings.json through the IConfiguration.


## Local configuration

Copy `appsettings.Example.json` to `appsettings.json` only when setting up a new checkout; do not overwrite an existing local configuration. The local file is ignored by Git. Fill in the connection string for your selected database provider, issuer, audience, and independent JWT signing/encryption keys. The example deliberately contains empty secrets and cannot start the application until configured.

Generate signing key material with `openssl rand -base64 48` and an encryption key with `openssl rand -base64 32`. Store the outputs only in your local configuration or secret manager. Never paste credentials into commits, logs, or issue reports.

For deployments, supply secrets through the hosting platform's secret manager or configuration, using `ConnectionStrings__DefaultConnection`, `Authentication__JwtSettings__SecretKey`, and `Authentication__JwtSettings__EncryptionKey` when using environment variables. All instances issuing or validating tokens must use the intended current keys.

## Previously committed credentials

Removing `appsettings.json` from Git tracking does not remove earlier commits or revoke credentials. Rotate the SQL login password on the database server and update every consumer's secret configuration. Replacing the password only in a connection string does not rotate the database login.

Replace both JWT keys in every deployed issuer/validator and restart or redeploy the application, which loads validation keys at startup. Removing the old keys invalidates tokens issued with them; users must sign in again. Local key replacement alone does not revoke keys still used by deployed instances.

After revocation, coordinate any Git history cleanup with repository collaborators; existing clones, forks, and artifacts may retain the old credentials. Do not restore credentials from history.

## Switching between PostgreSQL and Microsoft SQL Server (MSSQL)

Both provider packages are already referenced in `BaseEFAPI.csproj`. To switch databases, stop the API, select one provider in `Program.cs`, and update `ConnectionStrings:DefaultConnection` to match it. Keep only one active `AddDbContext<RegistrationDbContext>` registration.

### PostgreSQL to MSSQL

1. In `Program.cs`, comment out the active PostgreSQL `AddDbContext` block and uncomment the existing MSSQL block above it. Alternatively, replace the PostgreSQL block with:

   ```csharp
   builder.Services.AddDbContext<RegistrationDbContext>(options =>
       options.UseSqlServer(connectionString, sqlOptions =>
           sqlOptions.EnableRetryOnFailure(
               maxRetryCount: 5,
               maxRetryDelay: TimeSpan.FromSeconds(30),
               errorNumbersToAdd: null)));
   ```

2. In your local `appsettings.json`, set a SQL Server connection string. For example, using Windows authentication and a trusted server certificate:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=BaseEFAPI;Integrated Security=True;Encrypt=True;TrustServerCertificate=False"
   }
   ```

   Replace the server and database with your SQL Server instance and database. For SQL authentication, replace `Integrated Security=True` with `User ID=YOUR_USER;Password=YOUR_PASSWORD`. For a local development server using a self-signed certificate, `TrustServerCertificate=True` bypasses certificate validation; use a trusted certificate for deployment.

3. Prepare the target database using the instructions below, then restart the API.

### MSSQL to PostgreSQL

1. In `Program.cs`, comment out the MSSQL `AddDbContext` block and enable the PostgreSQL block:

   ```csharp
   builder.Services.AddDbContext<RegistrationDbContext>(options =>
       options.UseNpgsql(connectionString));
   ```

2. In your local `appsettings.json`, replace the SQL Server connection string with an Npgsql connection string:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=BaseEFAPI;Username=YOUR_USER;Password=YOUR_PASSWORD"
   }
   ```

   Replace the placeholders with your PostgreSQL connection details.

3. Prepare the target database using the instructions below, then restart the API.

### Prepare the selected database

Check whether `appsettings.Development.json`, user secrets, or the `ConnectionStrings__DefaultConnection` environment variable overrides your local connection string. The effective connection string must match the selected provider.

For either provider, if the target database is new and empty, run:

```sh
dotnet run --project BaseEFAPI.csproj -- --initialize-database
```

Then start the API normally:

```sh
dotnet run --project BaseEFAPI.csproj
```

When switching back to a database that already has the compatible Identity schema, start the API normally without initializing it again. For an existing SQL Server database with the older user schema, follow the SQL Server Identity upgrade instructions below. Existing PostgreSQL schemas require a reviewed PostgreSQL migration.

Switching providers changes which database the API reads and writes; it does not transfer users or other data between databases. If you need the same accounts on the new provider, plan a separate schema and data migration that preserves user IDs, password hashes, and Identity fields.

## PostgreSQL database setup

`Program.cs` currently selects PostgreSQL with `UseNpgsql`. Set `ConnectionStrings:DefaultConnection` to the intended PostgreSQL database using Npgsql connection-string syntax (Host, Port, Database, Username, Password).

For a new, empty database, create the mapped user table and all Identity claims/login/token tables once, then start the API normally:

```sh
dotnet run --project BaseEFAPI.csproj -- --initialize-database
```

This explicit command uses the EF model and exits without starting the server. It requires database/schema creation permissions. It does not run automatically, drop tables, or upgrade existing schemas. If any tables already exist, it fails with guidance instead of reporting successful initialization. This repository has no EF migrations; `EnsureCreatedAsync` does not establish migration history. Future schema changes require a planned migration approach.

If registration reports `42P01: relation "ApplicationUser" does not exist`, run these read-only queries against the same database used by the API:

```sql
SELECT current_database(), current_schema();
SHOW search_path;
SELECT table_schema, table_name
FROM information_schema.tables
WHERE lower(table_name) = 'applicationuser';
```

The model expects the exact quoted name `"ApplicationUser"`. An unquoted PostgreSQL table name becomes lowercase (`applicationuser`), which is a different name. If a user table already exists under another name or schema, review its columns and adapt the mapping or migrate the schema while preserving its data. Do not initialize or recreate that database as a substitute for an upgrade. The existing `--upgrade-identity` command and SQL script below support SQL Server only.
## Identity password policy and lockout

Registration uses `UserManager.CreateAsync(user, password)` and returns HTTP 400 with Identity validation errors when rejected. Sign-in accepts email or username (email takes precedence if both are supplied), calls `CheckPasswordSignInAsync` with `lockoutOnFailure: true`, and returns a generic HTTP 401 when denied. JWT generation happens only after a successful check. Accounts with two-factor authentication enabled are denied until a second-factor flow is implemented.

Policy is configured in `MVCS/Services/Authentication/IdentityServiceCollectionExtensions.cs`: the existing six-character, uppercase/lowercase/digit/symbol password requirements and five-failures/five-minute lockout now apply. These password requirements apply to new passwords, not retroactively to existing hashes. Lockout prevents new sign-ins; it does not revoke previously issued JWTs.

### Upgrade an existing database before deploying

Back up the database and stop application writers. Apply `Database/UpgradeApplicationUserToIdentity.sql` first if the database does not already have the Identity columns. Then, with configuration pointing to the intended database, run:

```sh
dotnet run --project BaseEFAPI.csproj -c Release -- --upgrade-identity
```

This explicit maintenance command uses Identity's normalizer to backfill existing usernames/emails, initializes missing stamps, enables lockout for existing accounts, adds unique identifier indexes and the Identity claims/login/token tables, then exits without starting the web server. Password hashes and current failed-attempt/lockout-end values are preserved. Missing, oversized, or duplicate identifiers cause the transaction to fail; resolve them before retrying. Review any pre-existing Identity tables/indexes for schema compatibility. For a large user table, plan a separate batched migration; this command loads users into memory.

The command requires schema-change permissions and is not run automatically on startup. Verify it against a restored SQL Server backup before production deployment. The automated tests use SQLite and do not validate SQL Server upgrade DDL.

Run authentication integration tests with `dotnet test Tests/BaseEFAPI.Tests.csproj`.

<!-- TECHNICAL INFORMATION -->
## Overview
[C#](https://learn.microsoft.com/en-us/dotnet/csharp/) | [EntityFramework](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli)

```
+ SCMs: GitHub Desktop
+ Programing Languages: C#
+ Frameworks: Entity Framework Core
+ Database: Microsoft SQL Database
+ IDEs: Visual Studios Community & Visual Studios Code
```

## Programs
* [Visual Studios Community](https://visualstudio.microsoft.com/vs/community/)
* [Visual Studios Code](https://code.visualstudio.com/)
* [GitHub Desktop](https://desktop.github.com/)


<!-- MARKDOWN LINKS & IMAGES || https://www.markdownguide.org/basic-syntax/#reference-style-links -->
<!--VERSION SHIELD-->
[version-shield]: https://img.shields.io/badge/Version-0.1-blueviolet
[version-url]: https://github.com/KTVo/BaseEFAPI

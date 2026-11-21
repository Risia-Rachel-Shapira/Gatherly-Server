# Gatherly

## About

Gatherly is an event management system that helps organizers manage events, participants, invitations, items, expenses, and contributions in one place.

Organizers create and manage events, invite participants, track shared items and expenses, and calculate each participant's equal share of the total costs.

## Technologies

### Backend
- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- PostgreSQL (Npgsql)
- JWT Authentication
- AutoMapper
- FluentValidation
- NLog

### Frontend
- HTML
- CSS
- JavaScript

### Testing
- xUnit
- Moq

## Architecture

The solution is divided into five projects:

- **Gatherly.API** – REST API, controllers, authentication, middleware, and configuration. Controllers only call services; they never use repositories directly.
- **Gatherly.Core** – Models, DTOs, repository and service interfaces, and shared exceptions (such as `ConflictException`).
- **Gatherly.Data** – Database context, repositories, migrations, and seed data.
- **Gatherly.Services** – Business logic, validation, and application services.
- **Gatherly.Tests** – Unit tests (xUnit + Moq) and an optional database concurrency test.

## Main Features

- User registration and login.
- JWT-based authentication.
- Event creation, update, deletion, and retrieval.
- Event organizer and participant permissions.
- Invitation creation and joining events.
- Participant management.
- Shared item management and item claims.
- Expense tracking.
- Participant contributions.
- Equal expense-share calculation.
- Pagination.
- Optimistic concurrency handling.
- Centralized exception handling and request correlation IDs.

## Data Model: the Many-to-Many Relationship

The many-to-many relationship in Gatherly is **Event ↔ User**, implemented through the join entity **`EventMember`**:

- An event has many members, and a user can be a member of many events.
- `EventMember` holds `EventId` and `UserId`, plus data about the membership itself: `Role` (`Participant`), `JoinedAt`, and `IsActive`. Because the join table carries its own data, it is an explicit entity rather than a hidden EF Core join table.
- A unique index on `(EventId, UserId)` ensures a user appears at most once per event. If an inactive member joins again through an invitation, the same row is reactivated (`IsActive = true`).
- The event organizer is a separate one-to-many relationship (`Event.OrganizerId` → `User`) and always has access to the event.

See [ERD.md](ERD.md) for the full diagram.

## Shared Limited Resource: Items and Claims

Each event has shared **items** (for example "Drinks"), and each item has a limited **`Quantity`**. Participants **claim** part of that quantity (`ItemClaim`). The core rule:

> The sum of all active claims on an item may never exceed the item's `Quantity`.

The difficult case is two participants claiming the last units of the same item **at the same time**. Both requests read the same remaining quantity, both pass the check, and without protection both would be saved, over-claiming the item.

Gatherly prevents this with **optimistic concurrency**:

1. `Item` has a `Version` (`Guid`) property configured as an EF Core **concurrency token** (`IsConcurrencyToken()`).
2. Every claim create, update, or cancel also marks the item as modified. On save, `AppDbContext` gives the item a new `Version`, and EF Core runs `UPDATE ... WHERE Id = @id AND Version = @originalVersion`.
3. The first request to save wins and changes the `Version`. The second request's `UPDATE` matches no rows, so EF Core throws `DbUpdateConcurrencyException`. The claim and the item update are saved in the same `SaveChanges` call, so nothing from the losing request is saved.
4. `Repository.SaveChangesAsync` catches only `DbUpdateConcurrencyException` and rethrows it as the domain `ConflictException` ("הפריט נתפס על ידי משתמש אחר. נסי שוב.").
5. `ExceptionMiddleware` maps `ConflictException` to **`409 Conflict`** (logged as a Warning) and returns a JSON body with the message and the request's correlation ID. The client can reload the item and try again.

A claim that simply asks for more than the remaining quantity is rejected before saving, also with **409** ("Not enough available quantity.").

## Running the Project

### Prerequisites

- .NET 8 SDK
- PostgreSQL running locally (default: `localhost:5432`, database `GatherlyDb`)

### 1. Configure secrets with `dotnet user-secrets`

Secrets are **not** stored in `appsettings.json`. For local development, use .NET user secrets (the API project already has a `UserSecretsId`). From the solution directory:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=GatherlyDb;Username=postgres;Password=<your-password>" --project Gatherly.API

dotnet user-secrets set "Jwt:Key" "<a-random-secret-of-at-least-32-characters>" --project Gatherly.API
```

- `ConnectionStrings:DefaultConnection` – the PostgreSQL connection string.
- `Jwt:Key` – the signing key for JWTs (HMAC-SHA256; use at least 32 characters). The API refuses to start without it.
- Optional: `Seed:AdminPassword` – the demo admin's password (see [Seed Data](#seed-data)).

Check the values with `dotnet user-secrets list --project Gatherly.API`. User secrets are loaded only in the Development environment. In other environments, use environment variables instead (for example `ConnectionStrings__DefaultConnection` and `Jwt__Key`).

### 2. Run the API

```bash
dotnet run --project Gatherly.API --launch-profile https
```

Or open `Gatherly.sln` in Visual Studio, set `Gatherly.API` as the startup project, and run it.

On startup, the database initializer applies any pending EF Core migrations and creates demo data when the database is empty, so there is no separate migration step.

| Profile | URL |
|---|---|
| HTTPS | https://localhost:7153 |
| HTTP | http://localhost:5141 |
| Swagger UI | https://localhost:7153/swagger |

Both launch profiles use the Development environment.

### 3. Run the client

Serve the frontend with a local development server (for example VS Code Live Server on `http://localhost:5500`, which is allowed by the API's CORS policy).

## Seed Data

**Demo Users:**

| User | Email | Password |
|---|---|---|
| Demo Organizer | organizer@gatherly.local | Organizer123! |
| Demo Participant | participant@gatherly.local | Participant123! |
| Demo Admin | admin@gatherly.local | Admin123! (Development only, see below) |

**Demo Event:**
- Name: Gatherly Demo Event
- Location: Tel Aviv
- Budget: 1000
- Management Type: Both

The initializer also creates two sample items, Drinks and Snacks, and adds the participant as an active event member.

**Note:** These credentials are for local development and testing only.

The demo admin is created on startup even if the database already has users. Its password is taken from `Seed:AdminPassword` (for example the environment variable `Seed__AdminPassword`). In the Development environment, if that setting is missing, the demo password `Admin123!` is used, so no setup is needed to run locally. In any other environment (e.g. Production) the setting is required, and without it the admin is not created. An existing account with that email is never promoted to Admin.

## Testing

Build and run all tests from the solution directory:

```bash
dotnet build
dotnet test
```

The unit tests use Moq and need no database.

### Running the concurrency test

`ItemConcurrencyTests` is an integration test against a real PostgreSQL database. It creates a temporary item, loads it in two separate `DbContext` instances, saves a change from the first, and checks that saving from the second throws `DbUpdateConcurrencyException`. This shows the `Version` concurrency token works in the real database. The temporary item is deleted at the end, even if the test fails; no other data is changed.

It is **skipped** unless the `GATHERLY_TEST_DB` environment variable is set to a PostgreSQL connection string. The database must already have the schema and at least one event (the temporary item is attached to an existing event and its organizer). Even so, use a development or test database, not production.

PowerShell:

```powershell
$env:GATHERLY_TEST_DB = "Host=localhost;Port=5432;Database=GatherlyDb;Username=postgres;Password=<your-password>"
dotnet test --filter "FullyQualifiedName~ItemConcurrencyTests"
```

Bash:

```bash
GATHERLY_TEST_DB="Host=localhost;Port=5432;Database=GatherlyDb;Username=postgres;Password=<your-password>" \
  dotnet test --filter "FullyQualifiedName~ItemConcurrencyTests"
```

## API Documentation

Swagger documents the REST API and lets you test the endpoints. After starting the API, open https://localhost:7153/swagger. See also [API-ENDPOINTS.md](API-ENDPOINTS.md).

## Authentication

The API uses JWT Bearer Authentication. Protected endpoints require a valid access token. Login checks the password in `UserService`, and `POST /api/Auth/register` returns `201 Created` with a token.

### Authorization Levels

Gatherly uses two separate levels of authorization:

- **Global role in the JWT (`Admin` / `User`).** The user's role is stored on the user and written into the token as a role claim. It controls access to system-wide endpoints and is enforced by ASP.NET Core with `[Authorize(Roles = "Admin")]`. A regular user gets `403 Forbidden` on these endpoints. All admin logic lives in `AdminService`:
  - `GET /api/admin/users?pageNumber=1&pageSize=10` – paged list of all users with their roles.
  - `GET /api/admin/events?pageNumber=1&pageSize=10` – paged list of all events in the system, not only the admin's own.
  - `DELETE /api/admin/events/{id}` – deletes any event, even when the admin is not its organizer, together with its related data (items, claims, expenses, contributions, invitations, members) in one transaction. Returns `204 No Content`, or `404 Not Found` if the event does not exist.
- **Event-level permission (`Organizer` / `Participant`).** Whether a user may view or change an event depends on data: who organizes that event and who is an active member of it. Because this cannot be known from the token alone, it is checked in the Service layer (`EventAccessService`) against the database on every request.

## Logging and Error Handling

- NLog writes logs to the console and to daily files (`logs/gatherly-<date>.log`).
- Every request gets a correlation ID: the `X-Correlation-ID` request header, or a new GUID. It is stored as the request's `TraceIdentifier`, returned in the `X-Correlation-ID` response header, included in error responses, and written to each log line (`${aspnet-TraceIdentifier}`).
- Log level is Debug in Development and Info in other environments. Framework (`Microsoft.*`) logs are limited to warnings.
- Passwords, JWTs, and invitation tokens are never logged (invitation tokens in URLs are masked).
- A global exception middleware returns consistent JSON errors:

| Status | When |
|---|---|
| 400 Bad Request | Validation errors (Data Annotations / FluentValidation, `ArgumentException`) |
| 401 Unauthorized | Missing or invalid token, wrong login |
| 403 Forbidden | `UnauthorizedAccessException` (no permission for the event or role) |
| 404 Not Found | `KeyNotFoundException` or a missing resource |
| 409 Conflict | `ConflictException`: concurrency conflicts and business conflicts (not enough quantity, email already registered) |
| 500 Internal Server Error | Any other error (details are logged, not returned) |

# Gatherly

## About
Gatherly is an event management system that helps organizers manage events, participants, invitations, items, expenses, and contributions in one place.

The system allows event organizers to create and manage events, invite participants, track shared items and expenses, and calculate each participant's equal share of the total costs.

## Technologies

### Backend
- C#
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- PostgreSQL
- Npgsql
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

- **Gatherly.API** – REST API, controllers, authentication, middleware, and configuration.
- **Gatherly.Core** – Core models, interfaces, DTOs, and application contracts.
- **Gatherly.Data** – Database context, repositories, migrations, and seed data.
- **Gatherly.Services** – Business logic, validation, and application services.
- **Gatherly.Tests** – Unit and integration-related tests.

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

## Database

The project uses PostgreSQL with Entity Framework Core Code First.

Database migrations are used to create and update the database schema.

Seed data provides initial users for local development.

## Running the Project

1. Make sure PostgreSQL is installed and running.
2. Configure the database connection string in the API configuration.
3. Open `Gatherly.sln` in Visual Studio.
4. Restore NuGet packages.
5. Apply Entity Framework Core migrations.
6. Run the `Gatherly.API` project.
7. Open the client using a local development server.

## Testing

Run the tests from the solution directory:

```bash
dotnet test
```

Build the solution:

```bash
dotnet build
```

## API Documentation

Swagger is configured to document and test the REST API endpoints.

After starting the API, open the Swagger UI using the configured Swagger URL.

## Authentication

The API uses JWT Bearer Authentication.

Protected endpoints require a valid access token.

Event permissions distinguish between organizers and active participants.

## Logging and Error Handling

- NLog is used for application logging.
- Logs are written to the console and daily log files.
- Correlation IDs are used to track requests.
- Global exception middleware provides consistent JSON error responses.
- HTTP status codes are used to represent validation, authorization, not-found, and conflict errors.


## Database
### Seed Data

The database initializer automatically applies pending migrations and creates initial demo data when the database is empty.

**Demo Users:**

| User | Email | Password |
|---|---|---|
| Demo Organizer | organizer@gatherly.local | Organizer123! |
| Demo Participant | participant@gatherly.local | Participant123! |

**Demo Event:**
- Name: Gatherly Demo Event
- Location: Tel Aviv
- Budget: 1000
- Management Type: Both

The initializer also creates two sample items: Drinks and Snacks, and adds the participant as an active event member.

**Note:** These credentials are for local development and testing only.

### Database Configuration

Gatherly uses PostgreSQL as its database.

Default development configuration:
- Host: localhost
- Port: 5432
- Database: GatherlyDb
- Provider: Npgsql

Before running the application, make sure PostgreSQL is running and the connection string is configured in `appsettings.json`.

### Running the Backend

1. Open `Gatherly.sln` in Visual Studio.
2. Make sure PostgreSQL is running.
3. Configure the database connection string.
4. Set `Gatherly.API` as the startup project.
5. Run the application.
6. Open the Swagger UI to explore and test the API.

### Running the Tests

Run the following command from the solution directory:

```bash
dotnet test
```

### API Configuration

The API runs locally using the following URLs:

| Profile | URL |
|---|---|
| HTTPS | https://localhost:7153 |
| HTTP | http://localhost:5141 |
| Swagger UI | https://localhost:7153/swagger |

The application uses the Development environment during local development.

### Database Requirements

- PostgreSQL must be installed and running.
- Database name: `GatherlyDb`
- Default PostgreSQL port: `5432`

The database initializer applies pending migrations and creates demo data when the database is empty.

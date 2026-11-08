# Gatherly – API Endpoints

## 1. Authentication – AuthController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| POST | `/api/Auth/register` | Register a new user | Public |
| POST | `/api/Auth/login` | Login and receive JWT token | Public |

## 2. Events – EventsController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/Events` | Get paginated events | Authenticated |
| GET | `/api/Events/{id}` | Get event by ID | Authenticated, event access required |
| POST | `/api/Events` | Create a new event | Authenticated |
| PUT | `/api/Events/{id}` | Update an event | Organizer only |
| DELETE | `/api/Events/{id}` | Delete an event | Organizer only |

## 3. Event Members – EventMembersController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/EventMembers/{eventId}` | Get active event members | Authenticated, event access required |

## 4. Invitations – InvitationsController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| POST | `/api/Invitations` | Create an event invitation | Authenticated, organizer only |
| GET | `/api/Invitations/{token}` | Get invitation by token | Authenticated |
| POST | `/api/Invitations/{token}/join` | Join an event using an invitation | Authenticated |

## 5. Items – ItemsController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/Items/{id}` | Get item by ID | Authenticated, event access required |
| GET | `/api/Items/event/{eventId}` | Get event items | Authenticated, event access required |
| POST | `/api/Items` | Create an item | Authenticated |
| PUT | `/api/Items/{id}` | Update an item | Authenticated |
| DELETE | `/api/Items/{id}` | Delete an item | Authenticated |

## 6. Item Claims – ItemClaimsController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/ItemClaims/item/{itemId}` | Get claims for an item | Authenticated |
| POST | `/api/ItemClaims` | Create an item claim | Authenticated |
| PUT | `/api/ItemClaims/{id}` | Update an item claim | Authenticated |
| DELETE | `/api/ItemClaims/{id}` | Cancel an item claim | Authenticated |

## 7. Expenses – ExpensesController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/Expenses/event/{eventId}` | Get event expenses | Authenticated |
| GET | `/api/Expenses/event/{eventId}/share` | Calculate equal expense share | Authenticated |
| POST | `/api/Expenses` | Create an expense | Authenticated |
| DELETE | `/api/Expenses/{id}` | Delete an expense | Authenticated |

## 8. Contributions – ContributionsController

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/Contributions/event/{eventId}` | Get event contributions | Authenticated |
| POST | `/api/Contributions` | Create a contribution | Authenticated |
| PUT | `/api/Contributions/{id}` | Update a contribution | Authenticated |
| DELETE | `/api/Contributions/{id}` | Delete a contribution | Authenticated |

## Authentication

Protected endpoints require a valid JWT Bearer token.

The token is obtained through the login endpoint and must be sent in the Authorization header:

`Authorization: Bearer {token}`

## HTTP Status Codes

| Status Code | Meaning |
|---|---|
| 200 | OK – Request completed successfully |
| 201 | Created – Resource created successfully |
| 204 | No Content – Update or deletion completed |
| 400 | Bad Request – Invalid request |
| 401 | Unauthorized – Authentication required or invalid |
| 403 | Forbidden – Access denied |
| 404 | Not Found – Resource not found |
| 409 | Conflict – Concurrency or business conflict |
| 500 | Internal Server Error – Unexpected server error |
# ikigai-api

Backend API for the Ikigai project — a guided self-discovery experience where a
player answers a series of "session" questions (Love, Skill, World, Paid) and
receives an AI-generated Ikigai analysis. The API persists player answers,
computes session scores, delegates AI analysis to an external n8n workflow,
and streams progress back to the frontend over Server-Sent Events (SSE).

## Tech Stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core 10** with **Npgsql** (PostgreSQL)
- **Swashbuckle** (Swagger / OpenAPI UI, Development only)
- **n8n** webhook integration for AI-driven Ikigai analysis
- Clean Architecture layering: `API` → `Application` → `Domain` ← `Infrastructure`

## Project Structure

| Project | Responsibility |
|---|---|
| `ikigai-api.API` | ASP.NET Core host: controllers, DI composition, `appsettings.json`, EF Core migrations |
| `ikigai-api.Application` | Services, DTOs, interfaces, business logic (scoring, n8n orchestration, SSE) |
| `ikigai-api.Domain` | Entities (`User`, `IkigaiResult`, session data) and repository interfaces |
| `ikigai-api.Infrastructure` | EF Core `ApplicationDbContext`, repository implementations |

## How to Run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A PostgreSQL database (connection string configured below)
- (Optional) Docker, if you want to run via container

### 1. Configure the database connection

The connection string lives in `ikigai-api.API/appsettings.json` under
`ConnectionStrings:DefaultConnection`. For local development, prefer
overriding it via `appsettings.Development.json` (gitignored) or
[user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
instead of committing real credentials:

```bash
cd ikigai-api.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

### 2. Apply EF Core migrations

```bash
dotnet tool install --global dotnet-ef   # first time only
dotnet ef database update --project ikigai-api.API
```

### 3. Run the API

```bash
dotnet run --project ikigai-api.API
```

- HTTP: `http://localhost:5112`
- HTTPS: `https://localhost:7242`
- Swagger UI available at `/swagger` when running in the `Development` environment.

### 4. Run via Docker

```bash
docker build -t ikigai-api .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." ikigai-api
```

The multi-stage `Dockerfile` builds and publishes `ikigai-api.API` on the
.NET 10 SDK image and runs it on the `aspnet:10.0` runtime image.

## Configuration

Key settings in `ikigai-api.API/appsettings.json`:

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string (Npgsql), with retry-on-failure and a 60s command timeout |
| `CorsSettings:AllowedOrigins` | Origins allowed in production CORS policy (dev allows any origin) |
| `N8nIntegration:WebhookUrl` | n8n workflow endpoint that performs the AI Ikigai analysis |
| `N8nIntegration:ApiKey` | Sent as `x-api-key` header on outbound requests to n8n |

> **Note:** `appsettings.json` currently contains real credentials (DB
> password, n8n API key) committed to the repo. Treat this as sensitive and
> avoid committing further secrets — move them to user-secrets, environment
> variables, or a secrets manager instead.

## API Overview

### User Progress (`api/user/progress`)

Players submit answers for each session in sequence. Each endpoint validates
input and persists the corresponding session data, keyed by `userId`.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/user/progress/prologue` | Creates the `User` and saves prologue reasons; returns the new `userId` |
| `POST` | `/api/user/progress/love` | Saves the "Love" session (hobbies, dream answer) |
| `POST` | `/api/user/progress/skill` | Saves the "Skill" session (hard/soft skills) |
| `POST` | `/api/user/progress/world` | Saves the "World Needs" session (gifts, calling) |
| `POST` | `/api/user/progress/paid` | Saves the "Paid For" session (job cards, monetizable experience) |

### Ikigai Processing (`api/ikigai`)

Orchestrates score calculation, the async n8n analysis call, and progress
streaming.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/ikigai/generate/{userId}` | Validates all 4 sessions are complete, computes scores, kicks off background n8n processing, returns a `processId` |
| `GET` | `/api/ikigai/stream/{processId}` | Opens an SSE stream; replays current status (`Completed`/`Failed`/`Processing`) then waits for live updates |
| `POST` | `/api/ikigai/webhook/update` | Callback consumed by the n8n workflow to report progress/results, which are persisted and pushed to connected SSE clients |

### Processing Flow

1. Frontend calls the 5 `user/progress` endpoints as the player completes each step.
2. Frontend calls `POST /api/ikigai/generate/{userId}`, receives a `processId`, and opens `GET /api/ikigai/stream/{processId}`.
3. The API computes session scores, saves an `IkigaiResult` (`Status = Pending`), and fires a background task that calls the n8n webhook with the full payload.
4. n8n performs the AI analysis and calls back `POST /api/ikigai/webhook/update` with progress/results.
5. The API persists results (`IkigaiSummary` rows per Ikigai component) and pushes updates to the client via the open SSE connection.
6. If n8n doesn't respond within 2 minutes, the process is marked `Failed` and the client is notified over SSE.

## Data Model

- `User` — root player entity, one-to-many with each session type and `IkigaiResult`
- `PrologueData`, `LoveSessionData`, `SkillSessionData`, `WorldSessionData`, `PaidSessionData` — one row per user per session, JSON-encoded list answers
- `IkigaiResult` — one per generation attempt (`Pending` → `Processing` → `Completed`/`Failed`), holds the 4 percentage scores and max-session category
- `IkigaiSummary` — one row per Ikigai component (`What You Love`, `Passion`, `Mission`, etc.) with AI-generated summaries and strengths/development points

All `User`-owned relationships cascade-delete; `IkigaiResult` → `IkigaiSummary` is a standard one-to-many.

## Migrations

Migrations live in `ikigai-api.API/Migrations/`. To add a new migration after
changing entities or `ApplicationDbContext`:

```bash
dotnet ef migrations add <MigrationName> --project ikigai-api.API
dotnet ef database update --project ikigai-api.API
```

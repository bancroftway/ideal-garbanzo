# MyApp - GitHub Copilot Instructions

This document provides context and architectural guidance for GitHub Copilot when working with the MyApp solution.

---

## Solution Overview

**MyApp** is a self-hosted workflow orchestration application built with:
- .NET 10 and C# 13+
- Azure Durable Task Framework (DTFx) with SQL Server persistence
- .NET Aspire for orchestration and observability
- ASP.NET Core Identity with social authentication
- Entity Framework Core for data access
- Blazor WebAssembly and Blazor Hybrid MAUI clients

---

## Architecture Diagram

### Complete Project Dependency Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          .NET Aspire AppHost                                │
│                        (MyApp.AppHost)                                      │
│                                                                             │
│  Orchestrates:                                                              │
│  - SQL Server (2 databases: durabletask, appdb)                            │
│  - WebApi (with authentication)                                            │
│  - Worker (3 replicas for horizontal scaling)                              │
│  - Client Apps (optional, for local development)                           │
└─────────────────────────────┬───────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐
    │   WebApi     │  │   Worker     │  │ ServiceDefaults  │
    │              │  │              │  │                  │
    │ - REST API   │  │ - DTFx Host  │  │ - Telemetry      │
    │ - Auth       │  │ - Activities │  │ - Health         │
    │ - Endpoints  │  │ - Workers    │  │ - Config         │
    └──────┬───────┘  └──────┬───────┘  └────────┬─────────┘
           │                 │                    │
           │                 │                    │
           └────────┬────────┘                    │
                    │                             │
                    ▼                             │
         ┌─────────────────────┐                  │
         │  Infrastructure     │◄─────────────────┘
         │                     │
         │ - Activities        │
         │ - DbContext         │
         │ - Configurations    │
         │ - Middleware        │
         │ - External Services │
         └──────────┬──────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │   Application       │
         │                     │
         │ - Orchestrations    │
         │ - Validators        │
         │ - Use Cases         │
         └──────────┬──────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │      Core           │
         │                     │
         │ - Entities          │
         │ - Interfaces        │
         │ - Domain Types      │
         └─────────────────────┘


    ┌──────────────────────────┐       ┌──────────────────────────┐
    │  Blazor WASM Client      │       │  Blazor Hybrid MAUI      │
    │                          │       │                          │
    │ - Web Browser            │       │ - iOS/Android/Windows    │
    │ - Social Auth            │       │ - Social Auth            │
    │ - Anonymous Home Page    │       │ - Anonymous Home Page    │
    └────────┬─────────────────┘       └────────┬─────────────────┘
             │                                  │
             │         References               │
             │         MyApp.Shared             │
             └────────┬─────────────────────────┘
                      │
                      ▼
           ┌─────────────────────┐
           │    Shared           │
           │                     │
           │ - DTOs              │
           │ - Routes            │
           │ - Components        │
           │ - Services          │
           └─────────────────────┘
```

### Data Flow Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Client Applications                          │
│                                                                      │
│  ┌────────────────────┐           ┌────────────────────┐            │
│  │  Blazor WASM       │           │  Blazor MAUI       │            │
│  │  (Browser)         │           │  (Mobile/Desktop)  │            │
│  └─────────┬──────────┘           └─────────┬──────────┘            │
│            │                                 │                       │
│            └────────────┬────────────────────┘                       │
│                         │                                            │
└─────────────────────────┼────────────────────────────────────────────┘
                          │
                          │ HTTPS (REST API)
                          │
                          ▼
┌──────────────────────────────────────────────────────────────────────┐
│                         WebApi Layer                                 │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  Authentication Middleware                                  │    │
│  │  - Cookie Authentication                                    │    │
│  │  - OAuth 2.0 (Google, Facebook, GitHub)                    │    │
│  └───────────────────────┬─────────────────────────────────────┘    │
│                          │                                           │
│  ┌───────────────────────▼───────────────────────┐                  │
│  │  Endpoints                                    │                  │
│  │  - POST /api/documents/ingest                 │                  │
│  │  - GET  /api/workflows/{id}/status            │                  │
│  │  - GET  /api/auth/login/{provider}            │                  │
│  │  - POST /api/auth/logout                      │                  │
│  │  - GET  /api/auth/user                        │                  │
│  └───────────────────────┬───────────────────────┘                  │
│                          │                                           │
│                          │ TaskHubClient                             │
│                          │                                           │
└──────────────────────────┼───────────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
┌────────────────┐  ┌──────────────────────────────────┐
│  SQL Server    │  │      Durable Task Framework      │
│                │  │                                  │
│  Database:     │  │  ┌────────────────────────────┐  │
│  - appdb       │◄─┼──│  Orchestration Instance    │  │
│                │  │  │  - IngestDocument          │  │
│  Tables:       │  │  └────────┬───────────────────┘  │
│  - Users       │  │           │                      │
│  - Documents   │  │           │ Fan-out              │
│  - AspNetUsers │  │           │                      │
│  - etc.        │  │  ┌────────┴────────┬─────────┐  │
└────────────────┘  │  │                 │         │  │
                    │  ▼                 ▼         ▼  │
┌────────────────┐  │ ┌──────┐      ┌──────┐  ┌──────┐│
│  SQL Server    │  │ │ Act1 │      │ Act2 │  │ Act3 ││
│                │  │ │Docling│     │Markit│  │Marker││
│  Database:     │  │ └──────┘      └──────┘  └──────┘│
│  - durabletask │◄─┤           Fan-in │              │
│                │  │                  ▼              │
│  Tables:       │  │            ┌──────────┐        │
│  - Instances   │  │            │  Summary │        │
│  - History     │  │            └──────────┘        │
│  - NewEvents   │  │                                │
│  - NewTasks    │  └────────────────────────────────┘
└────────────────┘                  ▲
                                    │
                        ┌───────────┴───────────┐
                        │                       │
                        ▼                       ▼
                 ┌──────────┐           ┌──────────┐
                 │ Worker 1 │           │ Worker 2 │  ...
                 │          │           │          │
                 │ - Polls  │           │ - Polls  │
                 │ - Executes│          │ - Executes│
                 └──────────┘           └──────────┘
```

### Authentication Flow with Social Providers

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│ Client App   │         │   WebApi     │         │  OAuth       │
│ (Blazor)     │         │              │         │  Provider    │
└──────┬───────┘         └──────┬───────┘         └──────┬───────┘
       │                        │                        │
       │ 1. Click "Login with   │                        │
       │    Google/FB/GitHub"   │                        │
       ├───────────────────────►│                        │
       │                        │                        │
       │                        │ 2. Redirect to OAuth   │
       │                        │    Authorization URL   │
       │                        ├───────────────────────►│
       │                        │                        │
       │                        │                        │ 3. User
       │                        │                        │    Authenticates
       │                        │                        │    & Approves
       │                        │                        │
       │                        │ 4. Callback with Code  │
       │                        │◄───────────────────────┤
       │                        │                        │
       │                        │ 5. Exchange Code for   │
       │                        │    Access Token        │
       │                        ├───────────────────────►│
       │                        │                        │
       │                        │ 6. Return Access Token │
       │                        │◄───────────────────────┤
       │                        │                        │
       │                        │ 7. Get User Profile    │
       │                        ├───────────────────────►│
       │                        │                        │
       │                        │ 8. Return Profile      │
       │                        │◄───────────────────────┤
       │                        │                        │
       │                        │ 9. Create/Update User  │
       │                        │    in appdb            │
       │                        │                        │
       │ 10. Set Auth Cookie &  │                        │
       │     Redirect to App    │                        │
       │◄───────────────────────┤                        │
       │                        │                        │
```

---

## Project Structure

### Solution Projects

```
MyApp.slnx
├── MyApp.AppHost                          [Aspire Orchestration]
├── MyApp.ServiceDefaults                  [Shared Configuration]
├── MyApp/
│   ├── MyApp.Server.Core                  [Domain Layer]
│   ├── MyApp.Server.Application           [Application Layer]
│   ├── MyApp.Server.Infrastructure        [Infrastructure Layer]
│   ├── MyApp.Server.WebApi                [Presentation - API]
│   ├── MyApp.Server.Worker                [Presentation - Worker]
│   ├── MyApp.Client.BlazorWasmApp         [Client - Web]
│   ├── MyApp.Client.BlazorHybridMobileApp [Client - Mobile/Desktop]
│   └── MyApp.Shared                       [Shared - DTOs/Components]
```

### Dependency References

| Project | References |
|---------|------------|
| **Core** | None (pure domain) |
| **Application** | Core, DTFx.Core |
| **Infrastructure** | Core, Application, EF Core, DTFx.SqlServer |
| **WebApi** | All above, ASP.NET Core, Identity, Auth providers |
| **Worker** | All server projects, DTFx hosting |
| **ServiceDefaults** | OpenTelemetry, Aspire packages |
| **AppHost** | Aspire.Hosting |
| **Shared** | Blazor components, minimal dependencies |
| **BlazorWasmApp** | Shared, Blazor WASM |
| **BlazorHybridMobileApp** | Shared, MAUI, Blazor Hybrid |

---

## Key Technologies

### Backend Stack

| Technology | Purpose |
|------------|---------|
| **.NET 10** | Runtime framework |
| **Durable Task Framework** | Workflow orchestration |
| **SQL Server** | Persistence (2 databases) |
| **ASP.NET Core Identity** | User authentication/authorization |
| **Entity Framework Core** | ORM for appdb |
| **.NET Aspire** | Application orchestration |
| **FluentValidation** | Input validation |
| **OpenTelemetry** | Observability |

### Frontend Stack

| Technology | Purpose |
|------------|---------|
| **Blazor WebAssembly** | Web client |
| **Blazor Hybrid (MAUI)** | Mobile/Desktop client |
| **AuthenticationStateProvider** | Client-side auth state |

### Authentication Providers

| Provider | Configuration Required |
|----------|------------------------|
| **Google** | Client ID, Client Secret |
| **Facebook** | App ID, App Secret |
| **GitHub** | Client ID, Client Secret |

---

## Database Schema

### Database: durabletask (Durable Task Framework)

| Table | Purpose |
|-------|---------|
| `dt.Instances` | Orchestration instance metadata |
| `dt.History` | Event history for deterministic replay |
| `dt.NewEvents` | Pending events queue |
| `dt.NewTasks` | Pending activity tasks |

### Database: appdb (Application Data)

| Table | Purpose |
|-------|---------|
| `Users` | Custom user profile data |
| `Documents` | Ingested documents |
| `AspNetUsers` | Identity user accounts |
| `AspNetRoles` | Identity roles |
| `AspNetUserClaims` | User claims |
| `AspNetUserLogins` | External OAuth logins |
| `AspNetUserRoles` | User-role mappings |
| `AspNetUserTokens` | OAuth tokens |

---

## API Endpoints

### Document Management

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/api/documents/ingest` | POST | Yes | Submit document for processing |
| `/api/workflows/{id}/status` | GET | Yes | Check workflow status |

### Authentication

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/api/auth/login/{provider}` | GET | No | Initiate OAuth login (google/facebook/github) |
| `/api/auth/callback` | GET | No | OAuth callback handler |
| `/api/auth/logout` | POST | Yes | Sign out user |
| `/api/auth/user` | GET | Yes | Get current user info |

### Documentation

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/openapi/v1.json` | GET | No | OpenAPI 3.1 spec (JSON) |
| `/openapi/v1.yaml` | GET | No | OpenAPI 3.1 spec (YAML) |
| `/scalar/v1` | GET | No | Interactive API docs (Scalar UI) |

---

## Client Application Routing

### Anonymous Access (Home Page)

Both Blazor WASM and Blazor Hybrid apps allow anonymous access to the home page:

```razor
@page "/"
@* No [Authorize] attribute - accessible to all *@

<h1>Welcome to MyApp</h1>

<AuthorizeView>
    <Authorized>
        <p>Hello, @context.User.Identity?.Name!</p>
    </Authorized>
    <NotAuthorized>
        <a href="/login">Sign in</a>
    </NotAuthorized>
</AuthorizeView>
```

### Protected Pages

All other pages require authentication:

```razor
@page "/documents"
@attribute [Authorize]

<h1>My Documents</h1>
```

---

## Configuration

### Connection Strings

```json
{
  "ConnectionStrings": {
    "durabletask": "Server=localhost;Database=durabletask;...",
    "appdb": "Server=localhost;Database=appdb;..."
  }
}
```

### Authentication Settings

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    },
    "Facebook": {
      "AppId": "...",
      "AppSecret": "..."
    },
    "GitHub": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  }
}
```

### Durable Task Settings

```json
{
  "DurableTask": {
    "TaskHubName": "DocumentIngestion",
    "MaxConcurrentActivities": 10,
    "MaxActiveOrchestrations": 100
  }
}
```

---

## Development Guidelines

### Clean Architecture Rules

1. **Core** layer has NO external dependencies
2. **Application** depends only on Core + DTFx abstractions
3. **Infrastructure** implements interfaces from Core/Application
4. **Presentation** (WebApi, Worker) coordinates everything

### Entity Framework Core

- Each entity has a configuration file in `Infrastructure/Data/Configurations/`
- Use `IEntityTypeConfiguration<T>` for fluent configuration
- Migrations are in the Infrastructure project
- Use Aspire EF Core hosting: `builder.AddSqlServerDbContext<ApplicationDbContext>("appdb")`

### Authentication

- WebApi handles all OAuth flows
- Clients store auth state via `AuthenticationStateProvider`
- Use `[Authorize]` attribute for protected pages/endpoints
- Home page is always anonymous

### Testing

- Unit tests for validators, activities, orchestrations
- Integration tests with SQL Server (containerized)
- Use `LocalOrchestrationService` for in-memory DTFx testing

---

## Common Commands

```bash
# Build solution
dotnet build MyApp.slnx

# Run with Aspire (starts all services)
dotnet run --project MyApp.AppHost

# Run specific project
dotnet run --project MyApp.Server.WebApi

# EF Core migrations
dotnet ef migrations add MigrationName --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi
dotnet ef database update --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi

# Run tests
dotnet test

# Lint OpenAPI document
spectral lint MyApp.Server.WebApi/MyApp.Server.WebApi.json
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| OAuth redirect fails | Verify redirect URIs in provider console match `https://localhost:xxxx/api/auth/callback` |
| User not authenticated | Check cookie settings, ensure HTTPS, verify auth middleware order |
| EF migrations fail | Ensure connection string is correct, database exists, and correct startup project |
| Worker not processing | Check SQL Server connection, TaskHubName consistency, and worker replicas |
| Orchestration stuck | Avoid non-deterministic code (DateTime.Now, Guid.NewGuid, random) in orchestrations |

---

## Key File Locations

| Purpose | Path |
|---------|------|
| Orchestrations | `MyApp.Server.Application/Workflows/` |
| Activities | `MyApp.Server.Infrastructure/Activities/` |
| DbContext | `MyApp.Server.Infrastructure/Data/ApplicationDbContext.cs` |
| Entity Configs | `MyApp.Server.Infrastructure/Data/Configurations/` |
| Auth Endpoints | `MyApp.Server.WebApi/Endpoints/AuthEndpoints.cs` |
| Document Endpoints | `MyApp.Server.WebApi/Endpoints/DocumentEndpoints.cs` |
| Blazor WASM | `MyApp.Client.BlazorWasmApp/` |
| Blazor MAUI | `MyApp.Client.BlazorHybridMobileApp/` |
| Shared Components | `MyApp.Shared/` |

---

## Aspire Integration

### Hosting Packages Used

| Package | Purpose |
|---------|---------|
| `Aspire.Hosting.SqlServer` | SQL Server container hosting |
| `Aspire.Microsoft.EntityFrameworkCore.SqlServer` | EF Core client integration |
| `Microsoft.Extensions.ServiceDiscovery` | Service discovery for Aspire |

### Aspire Dashboard

Available at: `https://localhost:15888` (or configured port)

Provides:
- Distributed tracing visualization
- Metrics and logs
- Resource health monitoring
- Container management

---

*For detailed architecture information, see [docs/architecture_plan.md](../docs/architecture_plan.md)*

# MyApp - GitHub Copilot Instructions

This document provides context and architectural guidance for GitHub Copilot when working with the MyApp solution.

---

## Solution Overview

**MyApp** is a self-hosted AI-powered workflow orchestration application built with:
- .NET 10 and C# 13+
- Microsoft Agent Framework Workflows with Redis checkpoint persistence
- AG-UI Protocol for real-time AI streaming
- .NET Aspire for orchestration and observability
- ASP.NET Core Identity with social authentication
- Entity Framework Core with PostgreSQL for data access
- React + CopilotKit v1.50 web frontend
- Capacitor for iOS/Android mobile apps

---

## Architecture Diagram

### Complete Project Dependency Tree

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          .NET Aspire AppHost                                │
│                        (MyApp.AppHost)                                      │
│                                                                             │
│  Orchestrates:                                                              │
│  - PostgreSQL (appdb database)                                              │
│  - Redis (workflow checkpoints and caching)                                 │
│  - WebApi (with authentication + AG-UI server)                              │
│  - Worker (single instance - Phase 1)                                       │
│  - React Frontend (Vite development server)                                 │
└─────────────────────────────┬───────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              ▼               ▼               ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐
    │   WebApi     │  │   Worker     │  │ ServiceDefaults  │
    │              │  │              │  │                  │
    │ - REST API   │  │ - Workflows  │  │ - Telemetry      │
    │ - AG-UI      │  │ - Activities │  │ - Health         │
    │ - Auth       │  │ - Redis      │  │ - Config         │
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
    │  React + CopilotKit      │       │  Capacitor Mobile        │
    │  (Web Browser)           │       │  (iOS/Android)           │
    │                          │       │                          │
    │ - AG-UI Protocol         │       │ - AG-UI Protocol         │
    │ - Social Auth            │       │ - Social Auth            │
    │ - CopilotKit v1.50       │       │ - Native Features        │
    └────────┬─────────────────┘       └────────┬─────────────────┘
             │                                  │
             │         AG-UI (SSE)              │
             │         /api/agent               │
             └────────┬─────────────────────────┘
                      │
                      ▼
           ┌─────────────────────┐
           │    WebApi + AG-UI   │
           │                     │
           │ - REST Endpoints    │
           │ - AG-UI Server      │
           │ - Authentication    │
           └─────────────────────┘
```

### Data Flow Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Client Applications                          │
│                                                                      │
│  ┌────────────────────┐           ┌────────────────────┐            │
│  │  React + CopilotKit│           │  Capacitor Mobile  │            │
│  │  (Browser)         │           │  (iOS/Android)     │            │
│  └─────────┬──────────┘           └─────────┬──────────┘            │
│            │                                 │                       │
│            └────────────┬────────────────────┘                       │
│                         │                                            │
└─────────────────────────┼────────────────────────────────────────────┘
                          │
                          │ AG-UI (SSE) + REST API
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
│  │  - POST /api/agent (AG-UI streaming)          │                  │
│  │  - POST /api/documents/ingest                 │                  │
│  │  - GET  /api/workflows/{id}/status            │                  │
│  │  - GET  /api/auth/login/{provider}            │                  │
│  │  - POST /api/auth/logout                      │                  │
│  │  - GET  /api/auth/user                        │                  │
│  └───────────────────────┬───────────────────────┘                  │
│                          │                                           │
│                          │ WorkflowExecutor                          │
│                          │                                           │
└──────────────────────────┼───────────────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
┌────────────────┐  ┌──────────────────────────────────┐
│  PostgreSQL    │  │   Microsoft Agent Framework      │
│                │  │                                  │
│  Database:     │  │  ┌────────────────────────────┐  │
│  - appdb       │◄─┼──│  Workflow Instance         │  │
│                │  │  │  - IngestDocument          │  │
│  Tables:       │  │  └────────┬───────────────────┘  │
│  - Users       │  │           │                      │
│  - Documents   │  │           │ Fan-out              │
│  - AspNetUsers │  │           │                      │
│  - etc.        │  │  ┌────────┴────────┬─────────┐  │
└────────────────┘  │  │                 │         │  │
                    │  ▼                 ▼         ▼  │
┌────────────────┐  │ ┌──────┐      ┌──────┐  ┌──────┐│
│  Redis         │  │ │ Act1 │      │ Act2 │  │ Act3 ││
│                │  │ │Docling│     │Markit│  │Marker││
│  Purpose:      │  │ └──────┘      └──────┘  └──────┘│
│  - Checkpoints │◄─┤           Fan-in │              │
│  - Cache       │  │                  ▼              │
│  - Pub/Sub     │  │            ┌──────────┐        │
│                │  │            │  Summary │        │
│                │  │            └──────────┘        │
│                │  │                                │
│                │  └────────────────────────────────┘
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
│ (React/Mobile│         │              │         │  Provider    │
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
│   └── MyApp.Shared                       [Shared - DTOs]
├── ui/                                    [React + CopilotKit Frontend]
└── mobile/                                [Capacitor iOS/Android]
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
| **Microsoft Agent Framework Workflows** | Workflow orchestration |
| **PostgreSQL** | Application database |
| **Redis** | Workflow checkpoints and caching |
| **AG-UI Protocol** | Real-time AI streaming (SSE) |
| **ASP.NET Core Identity** | User authentication/authorization |
| **Entity Framework Core** | ORM for appdb |
| **.NET Aspire** | Application orchestration |
| **FluentValidation** | Input validation |
| **OpenTelemetry** | Observability |

### Frontend Stack

| Technology | Purpose |
|------------|---------|
| **React + Vite** | Web application |
| **CopilotKit v1.50** | AI chat components |
| **Capacitor** | iOS/Android mobile apps |

### Authentication Providers

| Provider | Configuration Required |
|----------|------------------------|
| **Google** | Client ID, Client Secret |
| **Facebook** | App ID, App Secret |
| **GitHub** | Client ID, Client Secret |

---

## Database Schema

### PostgreSQL: appdb (Application Data)

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

### Redis (Workflow State)

| Key Pattern | Purpose |
|-------------|---------|
| `workflow:{id}:checkpoint:{name}` | Superstep checkpoint data |
| `workflow:{id}:state` | Current workflow state |

---

## API Endpoints

### AG-UI (AI Streaming)

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/api/agent` | POST | **Yes** | AG-UI streaming endpoint (SSE) |

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

## Configuration

### Connection Strings

```json
{
  "ConnectionStrings": {
    "appdb": "Host=localhost;Database=appdb;...",
    "redis": "localhost:6379"
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
  "Workflow": {
    "CheckpointTTLDays": 7
  }
}
```

---

## Development Guidelines

### Clean Architecture Rules

1. **Core** layer has NO external dependencies
2. **Application** depends only on Core + Agent Framework abstractions
3. **Infrastructure** implements interfaces from Core/Application
4. **Presentation** (WebApi, Worker) coordinates everything

### Agent Framework Middleware

The Microsoft Agent Framework provides native middleware support:

**Turn-Level Middleware (`IMiddleware`):**
- Intercepts all incoming activities (messages, events)
- Register via `builder.Services.AddSingleton<IMiddleware[]>([...])`
- Implement `OnTurnAsync(ITurnContext, NextDelegate, CancellationToken)`

**Callback Middleware (`CallbackMiddleware<T>`):**
- `CallbackMiddleware<AgentInvokeCallbackContext>` - AI agent invocations (PII, guardrails)
- `CallbackMiddleware<AgentFunctionInvocationCallbackContext>` - Tool/function calls
- Register via `.AsBuilder().UseCallbacks(config => config.AddCallback(...))`

**Built-in Middleware:**
- `TranscriptLoggerMiddleware` - Log all conversations to files/database
- `FileTranscriptLogger` - Default file-based transcript storage

**Centralized Error Handling:**
- Use `OnError(HandleErrorAsync)` in `AgentApplication` subclass
- Catches all unhandled exceptions for graceful error responses

**Workflow Step Instrumentation:**
- Workflows do NOT have built-in middleware
- Use manual OpenTelemetry instrumentation with `ActivitySource.StartActivity()`
- Use `WorkflowInstrumentation.ExecuteStepAsync()` helper for consistent patterns

### Entity Framework Core

- Each entity has a configuration file in `Infrastructure/Data/Configurations/`
- Use `IEntityTypeConfiguration<T>` for fluent configuration
- Migrations are in the Infrastructure project
- Use Aspire EF Core hosting: `builder.AddNpgsqlDbContext<ApplicationDbContext>("appdb")`

### Authentication

- WebApi handles all OAuth flows
- React frontend uses auth context provider
- Use `[Authorize]` attribute for protected endpoints
- AG-UI endpoint requires authentication

### Testing

- Unit tests for validators, activities, workflows
- Integration tests with PostgreSQL + Redis (containerized)
- Use in-memory storage for workflow logic testing

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

# Frontend development
cd ui
npm install
npm run dev

# Build mobile apps
npm run build
npx cap sync
npx cap open android
npx cap open ios
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| OAuth redirect fails | Verify redirect URIs in provider console match `https://localhost:xxxx/api/auth/callback` |
| User not authenticated | Check cookie settings, ensure HTTPS, verify auth middleware order |
| EF migrations fail | Ensure connection string is correct, database exists, and correct startup project |
| Worker not processing | Check Redis connection and worker logs |
| Workflow checkpoint missing | Check Redis connection and key TTL settings |
| AG-UI connection fails | Check CORS settings and authentication token |
| CopilotKit not connecting | Verify `runtimeUrl` and authentication headers |
| Mobile OAuth fails | Check deep link configuration in AndroidManifest.xml / Info.plist |

---

## Key File Locations

| Purpose | Path |
|---------|------|
| Workflows | `MyApp.Server.Application/Workflows/` |
| Activities | `MyApp.Server.Infrastructure/Activities/` |
| Agent Middleware | `MyApp.Server.Infrastructure/Middleware/` |
| Workflow Instrumentation | `MyApp.Server.Infrastructure/Telemetry/WorkflowInstrumentation.cs` |
| Redis Storage | `MyApp.Server.Infrastructure/Storage/RedisCheckpointStorage.cs` |
| DbContext | `MyApp.Server.Infrastructure/Data/ApplicationDbContext.cs` |
| Entity Configs | `MyApp.Server.Infrastructure/Data/Configurations/` |
| AG-UI Server | `MyApp.Server.WebApi/Program.cs` |
| Auth Endpoints | `MyApp.Server.WebApi/Endpoints/AuthEndpoints.cs` |
| Document Endpoints | `MyApp.Server.WebApi/Endpoints/DocumentEndpoints.cs` |
| React Frontend | `ui/` |
| Capacitor Config | `ui/capacitor.config.ts` |
| Mobile Platforms | `mobile/android/`, `mobile/ios/` |
| OpenAPI Document | `MyApp.Server.WebApi/MyApp.Server.WebApi.json` |

---

## Aspire Integration

### Hosting Packages Used

| Package | Purpose |
|---------|---------|
| `Aspire.Hosting.PostgreSQL` | PostgreSQL container hosting |
| `Aspire.Hosting.Redis` | Redis container hosting |
| `Aspire.Hosting.JavaScript` | Vite/React frontend hosting |

### Aspire Dashboard

Available at: `https://localhost:15888` (or configured port)

Provides:
- Distributed tracing visualization
- Metrics and logs
- Resource health monitoring
- Container management

---

*For detailed architecture information, see [docs/architecture_plan.md](../docs/architecture_plan.md)*

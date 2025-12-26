# MyApp Architecture Plan

> **Document Version:** 1.2  
> **Created:** December 15, 2025  
> **Last Updated:** December 16, 2025  
> **Status:** Implementation Complete

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Project Goals and Objectives](#2-project-goals-and-objectives)
3. [Technology Stack](#3-technology-stack)
4. [Solution Structure](#4-solution-structure)
5. [Architecture Overview](#5-architecture-overview)
6. [Durable Task Framework Investigation](#6-durable-task-framework-investigation)
7. [Worker Scaling Strategy](#7-worker-scaling-strategy)
8. [Sample Workflow: Document Ingestion](#8-sample-workflow-document-ingestion)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [API Design](#10-api-design)
11. [Testing Strategy](#11-testing-strategy)
12. [Configuration Management](#12-configuration-management)
13. [Deployment Considerations](#13-deployment-considerations)
14. [Future Enhancements](#14-future-enhancements)

---

## 1. Executive Summary

### 1.1 Purpose

This document describes the architecture and implementation plan for **MyApp**, a self-hosted workflow orchestration solution built using the Azure Durable Task Framework (DTFx) with SQL Server persistence. The solution demonstrates how to leverage DTFx for durable workflow execution without any dependency on Azure cloud services.

### 1.2 Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Orchestration Framework** | Azure Durable Task Framework (DTFx) | Production-proven, supports distributed execution, deterministic replay |
| **Persistence Backend** | SQL Server | Self-hosted, no Azure dependency, supports horizontal scaling |
| **Hosting Platform** | .NET Aspire | Simplified orchestration, built-in observability, container management |
| **Architecture Pattern** | Clean Architecture | Separation of concerns, testability, maintainability |
| **Validation** | FluentValidation | Declarative rules, auto-validation for endpoints |
| **Observability** | OpenTelemetry | Vendor-neutral, comprehensive tracing and metrics |
| **API Documentation** | OpenAPI 3.1 + Scalar UI | Native .NET 10 support, modern UI, build-time doc generation |

### 1.3 Constraints

- **No Azure cloud dependencies** - The solution must run entirely on self-hosted infrastructure
- **SQL Server only** - No Azure Storage, Azure Service Bus, or other Azure-specific backends
- **.NET 10** - Using the latest .NET runtime with preview language features
- **Zero build warnings** - All code must compile without warnings

---

## 2. Project Goals and Objectives

### 2.1 Primary Goals

1. **Investigate DTFx Capabilities** - Understand how the Durable Task Framework works outside of Azure Functions
2. **Enable Worker Scaling** - Demonstrate horizontal scaling by adding multiple worker instances
3. **Self-Hosted Solution** - Create a solution that can run entirely on-premises or in any cloud
4. **Production-Ready Patterns** - Implement patterns suitable for production use

### 2.2 Secondary Goals

1. **Comprehensive Observability** - Full tracing and metrics for workflow execution
2. **Clean Architecture** - Demonstrate proper layering and separation of concerns
3. **Robust Validation** - Strong input validation at API boundaries
4. **Idempotent Operations** - Support client-provided correlation IDs for idempotency

### 2.3 Non-Goals

- Integration with Azure Functions runtime
- Azure Storage or Service Bus backends
- Multi-tenancy support
- Workflow versioning strategies (deferred to future phases)

---

## 3. Technology Stack

### 3.1 Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Runtime framework |
| **C#** | Preview (13+) | Programming language |
| **.NET Aspire** | 13.0.2 | Application orchestration and observability |
| **SQL Server** | 2022+ | Workflow state persistence |

### 3.2 NuGet Packages

#### Durable Task Framework

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Azure.DurableTask.Core` | 3.6.0 | Core DTFx abstractions and runtime |
| `Microsoft.DurableTask.SqlServer` | 1.5.2 | SQL Server persistence provider |
| `Microsoft.Azure.DurableTask.Emulator` | 2.6.0 | In-memory emulator for testing |

#### Validation

| Package | Version | Purpose |
|---------|---------|---------|
| `FluentValidation` | 11.11.0 | Fluent validation rules |
| `FluentValidation.DependencyInjectionExtensions` | 11.11.0 | DI integration |
| `SharpGrip.FluentValidation.AutoValidation.Endpoints` | 1.5.0 | Auto-validation for minimal APIs |

#### Observability

| Package | Version | Purpose |
|---------|---------|---------|
| `OpenTelemetry.Extensions.Hosting` | 1.10.0 | Hosting integration |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.10.0 | OTLP exporter |
| `OpenTelemetry.Instrumentation.AspNetCore` | 1.10.1 | ASP.NET Core instrumentation |
| `OpenTelemetry.Instrumentation.Http` | 1.10.0 | HTTP client instrumentation |

#### API Documentation

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | Native OpenAPI 3.1 document generation |
| `Microsoft.Extensions.ApiDescription.Server` | 10.0.0 | Build-time OpenAPI document generation |
| `Scalar.AspNetCore` | 2.1.9 | Modern interactive API documentation UI |

#### Testing

| Package | Version | Purpose |
|---------|---------|---------||
| `xunit` | 2.9.2 | Test framework |
| `Moq` | 4.20.72 | Mocking framework |
| `FluentAssertions` | 6.12.2 | Fluent assertions |

#### Authentication

| Package | Version | Purpose |
|---------|---------|---------||
| `Microsoft.AspNetCore.Authentication.Google` | 10.0.0 | Google OAuth provider |
| `Microsoft.AspNetCore.Authentication.Facebook` | 10.0.0 | Facebook OAuth provider |
| `AspNet.Security.OAuth.GitHub` | 10.0.0 | GitHub OAuth provider |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.0 | ASP.NET Core Identity |

#### Entity Framework Core & Aspire

| Package | Version | Purpose |
|---------|---------|---------||
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 | EF Core SQL Server provider |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | Design-time EF Core tools |
| `Aspire.Microsoft.EntityFrameworkCore.SqlServer` | 9.1.0 | Aspire EF Core hosting for SQL Server |
| `Microsoft.Extensions.ServiceDiscovery` | 9.1.0 | Aspire client service discovery |

### 3.3 Development Tools

- **Visual Studio 2022** or **VS Code** with C# Dev Kit
- **Docker Desktop** for containerized SQL Server
- **Aspire Dashboard** for observability
- **Spectral** for OpenAPI document linting

---

## 4. Solution Structure

### 4.1 Directory Layout
Use the existing directory layout and modify as necessary

### 4.2 Project Responsibilities

| Project | Layer | Responsibility |
|---------|-------|----------------|
| **MyApp.Server.Core** | Domain | Entities, Interfaces, domain types, User entity |
| **MyApp.Server.Application** | Application | Orchestrations, validators, use cases |
| **MyApp.Server.Infrastructure** | Infrastructure | Activities, middleware, external integrations, DbContext, EF Core configurations |
| **MyApp.Server.WebApi** | Presentation | HTTP endpoints, API models, request handling, authentication |
| **MyApp.Server.Worker** | Presentation | Background worker, orchestration processing |
| **MyApp.AppHost** | Orchestration | Aspire host, resource provisioning |
| **MyApp.ServiceDefaults** | Cross-cutting | OpenTelemetry, health checks, service config |
| **MyApp.Tests** | Testing | Unit tests, integration tests |
| **MyApp.Client.BlazorHybridMobileApp** | Presentation | Maui Blazor Hybrid app with social authentication |
| **MyApp.Client.BlazorWasmApp** | Presentation | Blazor webassembly web app with social authentication |
| **MyApp.Shared** | Shared | Shared DTOs between server and client applications |

### 4.3 Dependency Flow

```
                    ┌─────────────────┐
                    │  MyApp.AppHost  │
                    │  (Orchestrates) │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
       ┌───────────┐  ┌───────────┐  ┌───────────────┐
       │  WebApi   │  │  Worker   │  │ServiceDefaults│
       └─────┬─────┘  └─────┬─────┘  └───────────────┘
             │              │                 ▲
             │              │                 │
             └──────┬───────┘                 │
                    │                         │
                    ▼                         │
         ┌─────────────────────┐              │
         │   Infrastructure    │──────────────┘
         └──────────┬──────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │    Application      │
         └──────────┬──────────┘
                    │
                    ▼
         ┌─────────────────────┐
         │       Core          │
         └─────────────────────┘
```

---

## 5. Architecture Overview

### 5.1 Clean Architecture Principles

The solution follows Clean Architecture (also known as Onion Architecture or Hexagonal Architecture):

1. **Independence of Frameworks** - The core business logic doesn't depend on ASP.NET Core or DTFx directly
2. **Testability** - Business rules can be tested without UI, database, or external services
3. **Independence of UI** - The API layer can be swapped without changing business logic
4. **Independence of Database** - The persistence mechanism is an implementation detail

### 5.2 Layer Descriptions

#### Core Layer (MyApp.Server.Core)

The innermost layer containing:
- **DTOs** (Data Transfer Objects) for workflow inputs and outputs
- **Interfaces** for services that infrastructure must implement
- **Domain Types** for business-specific value objects

**Key Rule:** This layer has NO external dependencies except for base .NET types.

#### Application Layer (MyApp.Server.Application)

Contains:
- **Orchestrations** - DTFx `TaskOrchestration<TInput, TOutput>` implementations
- **Validators** - FluentValidation rules for DTOs
- **Use Cases** - Application-specific business logic

**Dependencies:** Core layer only, plus DTFx Core abstractions.

#### Infrastructure Layer (MyApp.Server.Infrastructure)

Contains:
- **Activities** - DTFx `TaskActivity<TInput, TOutput>` implementations
- **Middleware** - Cross-cutting concerns (logging, metrics)
- **External Services** - File system, HTTP clients, databases
- **Hosting** - `DurableTaskHostedService` for worker lifecycle

**Dependencies:** Core, Application, plus external packages (DTFx, SQL Server provider).

#### Presentation Layer (MyApp.Server.WebApi, MyApp.Server.Worker)

Contains:
- **Endpoints** - HTTP request handlers
- **Models** - API-specific request/response models
- **Configuration** - Application startup and DI setup

**Dependencies:** All layers.

### 5.3 Durable Task Framework Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                          TaskHubClient                          │
│  (API Layer - Creates/Queries Orchestrations)                   │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SQL Server Database                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Instances   │  │   History    │  │    Queue     │          │
│  │    Table     │  │    Table     │  │    Table     │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                       TaskHubWorker(s)                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   Worker #1     │  │   Worker #2     │  │   Worker #3     │ │
│  │  Orchestration  │  │  Orchestration  │  │  Orchestration  │ │
│  │   + Activities  │  │   + Activities  │  │   + Activities  │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. Durable Task Framework Investigation

### 6.1 What is DTFx?

The **Durable Task Framework (DTFx)** is an open-source library for writing long-running, durable workflow orchestrations in .NET. Originally created by Microsoft for Azure Service Fabric and Azure Durable Functions, it can run standalone with various persistence backends.

### 6.2 Core Concepts

#### Orchestrations

- **Definition:** A `TaskOrchestration<TInput, TOutput>` that coordinates activities
- **Execution Model:** Deterministic replay - orchestrations re-execute on each event
- **State:** Persisted in the history table, replayed on worker restart
- **Constraints:** Must be deterministic (no `DateTime.Now`, `Guid.NewGuid()`, I/O)

```csharp
public class IngestDocumentOrchestration : TaskOrchestration<IngestDocumentRequest, IngestDocumentResponse>
{
    public override async Task<IngestDocumentResponse> RunTask(
        OrchestrationContext context,
        IngestDocumentRequest input)
    {
        // Schedule activities - deterministic replay-safe
        var result = await context.ScheduleTask<string>(
            typeof(SomeActivity), input.Data);
        
        return new IngestDocumentResponse { Result = result };
    }
}
```

#### Activities

- **Definition:** A `TaskActivity<TInput, TOutput>` that performs actual work
- **Execution Model:** At-least-once delivery with automatic retry
- **Side Effects:** Activities CAN have side effects (database writes, HTTP calls)
- **Idempotency:** Activities SHOULD be idempotent for retry safety

```csharp
public class SomeActivity : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string input)
    {
        // Perform actual work here
        return ProcessData(input);
    }
}
```

#### Middleware

- **Purpose:** Cross-cutting concerns without modifying orchestration/activity code
- **Types:** Orchestration dispatch middleware, Activity dispatch middleware
- **Pattern:** Static factory returning `Func<DispatchMiddlewareContext, Func<Task>, Task>`

```csharp
public static class LoggingMiddleware
{
    public static Func<DispatchMiddlewareContext, Func<Task>, Task> Create(ILogger logger)
    {
        return async (context, next) =>
        {
            logger.LogInformation("Starting {Name}", context.GetProperty<string>("name"));
            await next();
            logger.LogInformation("Completed {Name}", context.GetProperty<string>("name"));
        };
    }
}
```

### 6.3 Persistence Backends

| Backend | Use Case | Azure Dependency |
|---------|----------|------------------|
| **Azure Storage** | Azure cloud deployment | Yes |
| **Azure Service Bus** | High-throughput scenarios | Yes |
| **SQL Server** | Self-hosted, on-premises | No |
| **In-Memory (Emulator)** | Testing | No |
| **Netherite** | Extreme performance | Yes (Event Hubs) |

**This solution uses SQL Server** to avoid any Azure cloud dependencies.

### 6.4 SQL Server Schema

**IMPORTANT:** The solution uses **two separate SQL Server databases** to maintain clear separation of concerns:

1. **`durabletask` database** - DTFx workflow orchestration state (managed by DTFx)
2. **`appdb` database** - Application-specific data (managed by Entity Framework Core)

#### DTFx Database (`durabletask`)

The DTFx SQL Server provider automatically creates the following tables in the `durabletask` database:

| Table | Purpose |
|-------|---------|
| `dt.Instances` | Orchestration instance metadata |
| `dt.History` | Event history for deterministic replay |
| `dt.NewEvents` | Pending events queue |
| `dt.NewTasks` | Pending activity tasks |
| `dt.Versions` | Schema version tracking |

The schema is created automatically by calling `CreateIfNotExistsAsync()` on the orchestration service.

**Key Points:**
- Never manually modify DTFx tables
- DTFx manages its own schema migrations
- Connection string points to `durabletask` database only

#### Application Database (`appdb`)

See section 9.2 for Entity Framework Core configuration and application-specific tables (Users, Documents, Identity tables, etc.).

---

## 7. Worker Scaling Strategy

### 7.1 How DTFx Enables Scaling

DTFx is designed for horizontal scaling:

1. **Task Distribution** - Work items are stored in SQL Server queues
2. **Competing Consumers** - Multiple workers poll for work
3. **Lease-Based Locking** - Only one worker processes each task at a time
4. **Automatic Rebalancing** - If a worker crashes, its tasks are picked up by others

### 7.2 Aspire Configuration

Workers are scaled using Aspire's `WithReplicas()` method:

```csharp
// Program.cs in MyApp.AppHost
var builder = DistributedApplication.CreateBuilder(args);

// SQL Server with TWO separate databases
var sqlServer = builder.AddSqlServer("sql")
    .AddDatabase("durabletask")  // <-- DTFx workflow orchestration state only
    .AddDatabase("appdb");        // <-- Application data: Identity, Documents, and domain entities

// Worker with 3 replicas for scaling
var worker = builder.AddProject<Projects.MyApp_Worker>("worker")
    .WithReference(sqlServer)
    .WaitFor(sqlServer)
    .WithReplicas(3);  // <-- Scale to 3 instances

// WebApi for ingestion and authentication
builder.AddProject<Projects.MyApp_WebApi>("webapi")
    .WithReference(sqlServer)
    .WaitFor(sqlServer);

// Client apps (optional - for local development)
builder.AddProject<Projects.MyApp_Client_BlazorWasmApp>("blazorwasm")
    .WithReference(webapi);

builder.Build().Run();

// See .github/copilot-instructions.md for complete architecture diagram
```

### 7.3 Concurrency Configuration

Each worker's concurrency is configured in `appsettings.json`:

```json
{
  "DurableTask": {
    "TaskHubName": "DocumentIngestion",
    "MaxConcurrentActivities": 10,
    "MaxActiveOrchestrations": 100,
    "GracefulShutdownTimeout": "00:00:30"
  }
}
```

**Scaling Formula:**
- Total concurrent activities = `MaxConcurrentActivities × WorkerCount`
- With 3 workers × 10 activities each = 30 concurrent activities

### 7.4 Scaling Considerations

| Factor | Recommendation |
|--------|----------------|
| **CPU-bound activities** | Scale workers horizontally, set `MaxConcurrentActivities` = CPU cores |
| **I/O-bound activities** | Increase `MaxConcurrentActivities` per worker |
| **Long-running activities** | Consider separate activity-only workers |
| **Database connection limits** | Ensure SQL Server can handle connections to BOTH databases: `durabletask` (Workers × MaxConcurrentActivities) and `appdb` (WebApi + Worker connections for EF Core) |

### 7.5 Load Balancing

For the WebApi:
- Use a load balancer (NGINX, HAProxy, Kubernetes Ingress) for HTTP traffic
- All instances can create orchestrations via `TaskHubClient`
- Orchestration instance IDs ensure idempotency across API instances

For Workers:
- No load balancer needed - workers self-balance via SQL Server polling
- Add/remove replicas dynamically with Aspire or Kubernetes

---

## 8. Sample Workflow: Document Ingestion

### 8.1 Workflow Description

The **IngestDocument** workflow demonstrates a fan-out/fan-in pattern:

1. **Receive** document file via HTTP POST
2. **Fan-out** to 3 parallel activities (Docling, MarkitDown, Marker)
3. **Fan-in** results to summarization activity
4. **Return** final summary

### 8.2 Workflow Diagram

```
                    ┌──────────────────┐
                    │  HTTP POST       │
                    │  /api/documents  │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ IngestDocument   │
                    │  Orchestration   │
                    └────────┬─────────┘
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
          ▼                  ▼                  ▼
   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
   │   Docling   │    │  MarkitDown │    │   Marker    │
   │  Activity   │    │   Activity  │    │  Activity   │
   └──────┬──────┘    └──────┬──────┘    └──────┬──────┘
          │                  │                  │
          └──────────────────┼──────────────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │   Summarize      │
                    │   Activity       │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ IngestDocument   │
                    │   Response       │
                    └──────────────────┘
```

### 8.3 Implementation Details

#### Orchestration

```csharp
public class IngestDocumentOrchestration 
    : TaskOrchestration<IngestDocumentRequest, IngestDocumentResponse>
{
    public static class ActivityNames
    {
        public const string Docling = "IngestDocumentUsingDoclingActivity";
        public const string MarkitDown = "IngestDocumentUsingMarkitDownActivity";
        public const string Marker = "IngestDocumentUsingMarkerActivity";
        public const string Summarize = "SummarizeMarkdownActivity";
    }

    public override async Task<IngestDocumentResponse> RunTask(
        OrchestrationContext context,
        IngestDocumentRequest input)
    {
        var fileBase64 = Convert.ToBase64String(input.FileContent);

        // Fan-out: Execute all activities in parallel
        var doclingTask = context.ScheduleTask<string>(ActivityNames.Docling, fileBase64);
        var markitDownTask = context.ScheduleTask<string>(ActivityNames.MarkitDown, fileBase64);
        var markerTask = context.ScheduleTask<string>(ActivityNames.Marker, fileBase64);

        await Task.WhenAll(doclingTask, markitDownTask, markerTask);

        // Fan-in: Aggregate results and summarize
        var summarizeInput = new SummarizeMarkdownInput
        {
            DoclingResult = doclingTask.Result,
            MarkitDownResult = markitDownTask.Result,
            MarkerResult = markerTask.Result
        };

        var summary = await context.ScheduleTask<string>(ActivityNames.Summarize, summarizeInput);

        return new IngestDocumentResponse
        {
            CorrelationId = input.CorrelationId,
            Summary = summary,
            ProcessedAt = context.CurrentUtcDateTime
        };
    }
}
```

### 8.4 Activity Implementations

Each activity is a placeholder that simulates document processing:

| Activity | Input | Output | Purpose |
|----------|-------|--------|---------|
| `IngestDocumentUsingDoclingActivity` | Base64 file | Markdown | Convert using IBM Docling |
| `IngestDocumentUsingMarkitDownActivity` | Base64 file | Markdown | Convert using Microsoft MarkitDown |
| `IngestDocumentUsingMarkerActivity` | Base64 file | Markdown | OCR using Marker library |
| `SummarizeMarkdownActivity` | 3 markdown texts | Summary | Combine and summarize |

---

## 9. Cross-Cutting Concerns

### 9.1 Authentication & Authorization

#### Social Authentication Providers

The application supports authentication through multiple social providers:

| Provider | Client Apps | WebApi | Configuration |
|----------|-------------|--------|---------------|
| **Google** | ✅ | ✅ | OAuth 2.0 via Google Cloud Console |
| **Facebook** | ✅ | ✅ | OAuth 2.0 via Facebook Developer Portal |
| **GitHub** | ✅ | ✅ | OAuth 2.0 via GitHub Apps |

#### Authentication Flow

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│ Client App   │         │   WebApi     │         │  Provider    │
│ (Blazor)     │         │              │         │ (Google/FB)  │
└──────┬───────┘         └──────┬───────┘         └──────┬───────┘
       │                        │                        │
       │ 1. Click Login         │                        │
       ├───────────────────────>│                        │
       │                        │ 2. Redirect to OAuth   │
       │                        ├───────────────────────>│
       │                        │                        │
       │                        │ 3. User Authenticates  │
       │                        │<───────────────────────│
       │                        │                        │
       │ 4. Return with Token   │                        │
       │<───────────────────────│                        │
       │                        │                        │
       │ 5. Store Auth State    │                        │
       │                        │                        │
```

#### Identity Configuration

**WebApi (Program.cs):**
```csharp
// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
});

builder.Services.AddAuthorization();
```

#### Client-Side Authentication

**Blazor WASM (Program.cs):**
```csharp
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
```

**Blazor Hybrid MAUI (MauiProgram.cs):**
```csharp
builder.Services.AddAuthorizationCore();
builder.Services.AddMauiBlazorWebView();
builder.Services.AddScoped<AuthenticationStateProvider, MauiAuthenticationStateProvider>();
```

#### Page-Level Authorization

**Home Page (Anonymous Access):**
```razor
@page "/"
@* No @attribute [Authorize] - accessible to all *@

<h1>Welcome to MyApp</h1>
<p>This page is accessible to everyone.</p>

<AuthorizeView>
    <Authorized>
        <p>Hello, @context.User.Identity?.Name!</p>
    </Authorized>
    <NotAuthorized>
        <p><a href="/login">Sign in</a> to access more features.</p>
    </NotAuthorized>
</AuthorizeView>
```

**Protected Page (Requires Authentication):**
```razor
@page "/documents"
@attribute [Authorize]

<h1>My Documents</h1>
@* Only authenticated users can access this page *@
```

#### Authentication Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------||
| `/api/auth/login/{provider}` | GET | Initiate OAuth flow (provider: google, facebook, github) |
| `/api/auth/callback` | GET | OAuth callback handler |
| `/api/auth/logout` | POST | Sign out user and clear session |
| `/api/auth/user` | GET | Get current user information |

#### Configuration Keys

```json
// appsettings.json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret"
    },
    "Facebook": {
      "AppId": "your-app-id",
      "AppSecret": "your-app-secret"
    },
    "GitHub": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

**Security Considerations:**
- Store secrets in Azure Key Vault, AWS Secrets Manager, or user secrets for development
- Use HTTPS for all authentication flows
- Implement CSRF protection for state tokens
- Set appropriate cookie security flags (HttpOnly, Secure, SameSite)

### 9.2 Entity Framework Core

#### DbContext Configuration

**ApplicationDbContext.cs:**
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    // Additional DbSets for domain entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

#### Entity Configuration Files

Each entity has a dedicated configuration file following the pattern:

**Data/Configurations/DocumentConfiguration.cs:**
```csharp
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(d => d.RepositoryName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(d => d.CorrelationId)
            .IsUnique();
            
        // Add relationships, indexes, etc.
    }
}
```

#### Aspire EF Core Integration

**WebApi (Program.cs):**
```csharp
// Use Aspire EF Core hosting package
builder.AddSqlServerDbContext<ApplicationDbContext>("appdb", 
    configureDbContextOptions: options =>
    {
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    });
```

**Required NuGet Packages:**
- `Aspire.Microsoft.EntityFrameworkCore.SqlServer` (9.1.0) - Hosting integration
- `Microsoft.Extensions.ServiceDiscovery` (9.1.0) - Service discovery client

**Migration Commands:**
```bash
# Add migration
dotnet ef migrations add InitialCreate --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi

# Update database
dotnet ef database update --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi
```

#### Repository Pattern (Optional)

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Implementation...
}
```

### 9.3 Logging

**Approach:** Middleware-based, not base class inheritance

```csharp
// Correct: Use middleware
worker.AddActivityDispatcherMiddleware(LoggingActivityMiddleware.Create(logger));

// Wrong: Don't use base classes
public abstract class LoggedActivity<T,R> : TaskActivity<T,R> // ❌
```

**Log Levels:**
- `Information` - Orchestration/activity start and completion
- `Warning` - Retries, soft failures
- `Error` - Exceptions, hard failures

### 9.2 OpenTelemetry Integration

#### Activity Source and Meter

```csharp
public static class WorkflowTelemetry
{
    public static readonly ActivitySource Source = new("MyApp.Workflows");
    public static readonly Meter Meter = new("MyApp.Workflows");
}
```

#### Trace Span Naming Convention

| Pattern | Example |
|---------|---------|
| Workflow execution | `workflow.execute` |
| Activity execution | `workflow.activity.execute` |

#### Metric Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| Counter | Plural noun | `workflow.executions` |
| Histogram | `.duration` suffix | `workflow.activity.duration` |
| UpDownCounter | Singular noun | `workflow.active` |

### 9.5 Input/Output Recording

**Captured as span events** for proper timestamping:

```csharp
public static void RecordInput<T>(Activity activity, T input) where T : class
{
    var json = JsonSerializer.Serialize(input);
    var tags = new ActivityTagsCollection
    {
        ["input.type"] = typeof(T).Name,
        ["input.size"] = json.Length,
        ["input.preview"] = Truncate(json, 1024)
    };
    activity.AddEvent(new ActivityEvent("input.recorded", tags: tags));
}
```

**Sensitive Data Redaction:**
- Automatically redact fields containing: `password`, `secret`, `apikey`, `token`, `credential`
- Truncate large payloads to 4KB, preview at 1KB

### 9.6 Performance Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `workflow.executions` | Counter | Total orchestrations started |
| `workflow.execution.duration` | Histogram | Orchestration duration in seconds |
| `workflow.activity.executions` | Counter | Total activities executed |
| `workflow.activity.duration` | Histogram | Activity duration in seconds |
| `workflow.errors` | Counter | Total errors (tagged by type) |

---

## 10. API Design

### 10.1 Endpoints

#### POST /api/documents/ingest

**Purpose:** Submit a document for processing

**Request:**
```http
POST /api/documents/ingest HTTP/1.1
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary

------WebKitFormBoundary
Content-Disposition: form-data; name="file"; filename="document.pdf"
Content-Type: application/pdf

<binary file content>
------WebKitFormBoundary
Content-Disposition: form-data; name="repositoryName"

my-repo
------WebKitFormBoundary
Content-Disposition: form-data; name="correlationId"

optional-client-id-12345
------WebKitFormBoundary--
```

**Response (202 Accepted):**
```json
{
  "instanceId": "optional-client-id-12345",
  "statusUrl": "/api/workflows/optional-client-id-12345/status"
}
```

**Validation Rules:**
- `file`: Required IFormFile, max 100MB (filename obtained from `IFormFile.FileName`)
- `repositoryName`: Required, non-empty
- `correlationId`: Optional, used as instance ID if provided

#### GET /api/workflows/{instanceId}/status

**Purpose:** Check orchestration status

**Response (200 OK):**
```json
{
  "instanceId": "optional-client-id-12345",
  "status": "Completed",
  "createdTime": "2025-12-15T10:00:00Z",
  "lastUpdatedTime": "2025-12-15T10:00:15Z",
  "output": {
    "correlationId": "optional-client-id-12345",
    "summary": "Document contains...",
    "processedAt": "2025-12-15T10:00:15Z"
  }
}
```

**Response (404 Not Found):**
```json
{
  "error": "Orchestration not found",
  "instanceId": "unknown-id"
}
```

#### Authentication Endpoints

##### GET /api/auth/login/{provider}

**Purpose:** Initiate OAuth login flow

**Parameters:**
- `provider`: OAuth provider name (google, facebook, github)

**Response:** Redirect to OAuth provider's authorization URL

##### GET /api/auth/callback

**Purpose:** OAuth callback handler (automatically invoked by OAuth provider)

**Response:** Sets authentication cookie and redirects to application

##### POST /api/auth/logout

**Purpose:** Sign out current user

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

##### GET /api/auth/user

**Purpose:** Get current authenticated user information

**Response (200 OK):**
```json
{
  "id": "user-id",
  "email": "user@example.com",
  "name": "User Name",
  "isAuthenticated": true
}
```

**Response (401 Unauthorized):**
```json
{
  "isAuthenticated": false
}
```

### 10.2 Validation

Using **FluentValidation** with auto-validation via endpoint group filter:

```csharp
public class IngestDocumentApiRequestValidator : AbstractValidator<IngestDocumentApiRequest>
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Initializes validation rules for document ingestion requests.
    /// </summary>
    public IngestDocumentApiRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)}MB")
            .When(x => x.File is not null);

        RuleFor(x => x.RepositoryName)
            .NotEmpty()
            .WithMessage("RepositoryName is required");

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("CorrelationId cannot be empty when provided")
            .When(x => x.CorrelationId is not null);
    }
}
```

#### Auto-Validation Configuration

Auto-validation is enabled per endpoint group using `SharpGrip.FluentValidation.AutoValidation.Endpoints`:

```csharp
// DocumentEndpoints.cs
var group = endpoints.MapGroup("/api/documents")
    .WithTags("Documents")
    .AddFluentValidationAutoValidation();  // Automatic validation filter

group.MapPost("/ingest", IngestDocumentAsync)
    .WithName("IngestDocument")
    .WithSummary("Ingest a document for processing")
    .Accepts<IngestDocumentApiRequest>("multipart/form-data")
    .Produces<IngestDocumentApiResponse>(StatusCodes.Status202Accepted);
```

### 10.3 API Documentation

#### OpenAPI 3.1 Support

The WebAPI uses native .NET 10 OpenAPI 3.1 support with build-time document generation:

```csharp
// Program.cs
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "MyApp API";
        document.Info.Version = "v1";
        document.Info.Description = "Document ingestion and workflow orchestration API...";
        document.Info.Contact = new() { Name = "MyApp Team", Email = "support@myapp.local" };
        return Task.CompletedTask;
    });
});
```

#### Endpoint Configuration (MyApp.Server.WebApi.csproj)

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)</OpenApiDocumentsDirectory>
  <OpenApiGenerateDocuments>true</OpenApiGenerateDocuments>
  <OpenApiGenerateDocumentsOptions>--openapi-version OpenApi3_1</OpenApiGenerateDocumentsOptions>
</PropertyGroup>
```

#### Available Endpoints

| Endpoint | Purpose |
|----------|---------||
| `/openapi/v1.json` | OpenAPI 3.1 document (JSON) |
| `/openapi/v1.yaml` | OpenAPI 3.1 document (YAML) |
| `/scalar/v1` | Scalar interactive API documentation UI |

#### Spectral Linting

OpenAPI documents are linted using Spectral (`.spectral.yml`):

```yaml
extends: ["spectral:oas"]
rules:
  oas3-api-servers: off  # Servers populated at runtime
  operation-operationId: warn
  operation-description: warn
```

Run linting:
```bash
spectral lint src/MyApp.Server.WebApi/MyApp.Server.WebApi.json
```

---

## 11. Testing Strategy

### 11.1 Test Categories

| Category | Scope | Framework | Database |
|----------|-------|-----------|----------|
| **Unit Tests** | Single class | xUnit + Moq | None |
| **Integration Tests** | Workflow execution | xUnit | SQL Server (containerized) |
| **Emulator Tests** | Orchestration logic | LocalOrchestrationService | In-memory |

### 11.2 Unit Test Examples

#### Validator Tests

```csharp
[Fact]
public void Validate_EmptyFileName_ShouldFail()
{
    var request = CreateValidRequest();
    request.FileName = "";

    var result = _validator.TestValidate(request);

    result.ShouldHaveValidationErrorFor(x => x.FileName);
}
```

#### Telemetry Tests

```csharp
[Fact]
public void RecordInput_ShouldAddEventWithCorrectTags()
{
    using var activity = new Activity("test").Start();
    var input = new TestInput { Value = "test" };

    TelemetryHelpers.RecordInput(activity, input);

    var evt = activity.Events.Single(e => e.Name == "input.recorded");
    Assert.Equal("TestInput", evt.Tags.First(t => t.Key == "input.type").Value);
}
```

### 11.3 Integration Tests (Future)

For comprehensive testing with real SQL Server:

```csharp
public class IntegrationTests : IAsyncLifetime
{
    private SqlOrchestrationService _service;
    private TaskHubWorker _worker;
    private TaskHubClient _client;

    public async Task InitializeAsync()
    {
        var settings = new SqlOrchestrationServiceSettings(
            connectionString: "Server=localhost;Database=durabletask-test;...",
            taskHubName: "TestHub");
        
        _service = new SqlOrchestrationService(settings);
        await _service.CreateIfNotExistsAsync();
        
        _worker = new TaskHubWorker(_service);
        _worker.AddTaskOrchestrations(typeof(IngestDocumentOrchestration));
        _worker.AddTaskActivities(/* real activities */);
        await _worker.StartAsync();
        
        _client = new TaskHubClient(_service);
    }

    [Fact]
    public async Task FullWorkflow_ShouldComplete()
    {
        var request = new IngestDocumentRequest { /* ... */ };
        
        var instance = await _client.CreateOrchestrationInstanceAsync(
            typeof(IngestDocumentOrchestration),
            request.CorrelationId,
            request);
        
        var result = await _client.WaitForOrchestrationAsync(instance, TimeSpan.FromMinutes(5));
        
        Assert.Equal(OrchestrationStatus.Completed, result.OrchestrationStatus);
    }
}
```

---

## 12. Configuration Management

### 12.1 Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "DurableTask": {
    "TaskHubName": "DocumentIngestion-Dev"
  },
  "OpenTelemetry": {
    "SamplingRatio": 1.0
  }
}

// appsettings.Production.json
{
  "DurableTask": {
    "TaskHubName": "DocumentIngestion"
  },
  "OpenTelemetry": {
    "SamplingRatio": 0.1
  }
}
```

### 12.3 Connection Strings

**Two Separate Databases Required:**
- **`durabletask`** - DTFx workflow orchestration state (used by Worker and WebApi for TaskHubClient)
- **`appdb`** - Application data, ASP.NET Core Identity, domain entities (used by WebApi and Worker via EF Core)

**Development (via Aspire):**
```json
{
  "ConnectionStrings": {
    "durabletask": "Server={sql.bindings.tcp.host},{sql.bindings.tcp.port};Database=durabletask;User Id=sa;Password=...;TrustServerCertificate=True",
    "appdb": "Server={sql.bindings.tcp.host},{sql.bindings.tcp.port};Database=appdb;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```2

**Production:**
```json
{
  "ConnectionStrings": {
    "durabletask": "Server=sql-prod.internal;Database=durabletask;User Id=dtfx_user;Password=...;Encrypt=True",
    "appdb": "Server=sql-prod.internal;Database=appdb;User Id=app_user;Password=...;Encrypt=True"
  }
}
```

**Production Best Practices:**
- Use separate service accounts for each database (`dtfx_user` vs `app_user`)
- Grant `dtfx_user` only necessary permissions on `durabletask` database
- Grant `app_user` only necessary permissions on `appdb` database
- Consider separate SQL Server instances for additional isolation if needed
```

---

## 13. Deployment Considerations

### 13.1 Development (Aspire)

```bash
# Start all services
dotnet run --project MyApp.AppHost

# Dashboard available at https://localhost:15888
```

### 13.2 Docker Compose

```yaml
version: '3.8'
services:
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: ${SQL_PASSWORD}
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql

  webapi:
    image: myapp-webapi:latest
    environment:
      # DTFx database for workflow orchestration
      ConnectionStrings__durabletask: Server=sql;Database=durabletask;...
      # Application database for Identity, Documents, etc.
      ConnectionStrings__appdb: Server=sql;Database=appdb;...
      Authentication__Google__ClientId: ${GOOGLE_CLIENT_ID}
      Authentication__Google__ClientSecret: ${GOOGLE_CLIENT_SECRET}
    ports:
      - "8080:8080"
    depends_on:
      - sql

  worker:
    image: myapp-worker:latest
    environment:
      # DTFx database for workflow orchestration
      ConnectionStrings__durabletask: Server=sql;Database=durabletask;...
      # Application database for accessing domain entities in activities
      ConnectionStrings__appdb: Server=sql;Database=appdb;...
    depends_on:
      - sql
    deploy:
      replicas: 3

volumes:
  sql-data:
```

### 13.3 Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-worker
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp-worker
  template:
    metadata:
      labels:
        app: myapp-worker
    spec:
      containers:
      - name: worker
        image: myapp-worker:latest
        env:
        - name: ConnectionStrings__durabletask
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: sql-connection-string
        resources:
          requests:
            cpu: 500m
            memory: 256Mi
          limits:
            cpu: 1000m
            memory: 512Mi
```

---

## 14. Future Enhancements

### 14.1 Phase 2: Production Hardening

| Item | Description | Priority |
|------|-------------|----------|
| **Retry Policies** | Configure activity retry with exponential backoff | High |
| **Dead Letter Queue** | Handle poison messages | High |
| **Workflow Versioning** | Support side-by-side orchestration versions | Medium |
| **Sub-Orchestrations** | Break large workflows into child orchestrations | Medium |

### 14.2 Phase 3: Advanced Features

| Item | Description | Priority |
|------|-------------|----------|
| **External Events** | Human-in-the-loop approval workflows | Medium |
| **Timers** | Scheduled activities and timeouts | Medium |
| **ContinueAsNew** | Eternal orchestrations with state cleanup | Low |
| **Saga Pattern** | Compensating transactions for failures | Low |

### 14.3 Phase 4: Operations

| Item | Description | Priority |
|------|-------------|----------|
| **Grafana Dashboards** | Pre-built observability dashboards | High |
| **Alerting** | Prometheus alerts for workflow failures | High |
| **Admin API** | Orchestration management endpoints | Medium |
| **Purging** | Automatic cleanup of completed orchestrations | Medium |

---

## Appendix A: Architecture Diagram

For a complete visual representation of the solution architecture, including all projects and their dependency tree, see:

**[.github/copilot-instructions.md](.github/copilot-instructions.md)**

The diagram includes:
- All solution projects (WebApi, Worker, Client apps, Shared libraries)
- Project dependencies and references
- Data flow between components
- Database connections (durabletask and appdb)
- Authentication flow with social providers
- Aspire orchestration architecture

This diagram is kept up-to-date with solution changes and serves as the source of truth for understanding project relationships.

---

## Appendix B: Quick Reference

### B.1 Common Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Start with Aspire
dotnet run --project MyApp.AppHost

# Run specific project
dotnet run --project src/MyApp.Server.WebApi

# Lint OpenAPI document
spectral lint src/MyApp.Server.WebApi/MyApp.Server.WebApi.json
```

### B.2 Key File Locations

| Purpose | Path |
|---------|------|
| Orchestration | `src/MyApp.Server.Application/Workflows/IngestDocumentOrchestration.cs` |
| DbContext | `src/MyApp.Server.Infrastructure/Data/ApplicationDbContext.cs` |
| Entity Configurations | `src/MyApp.Server.Infrastructure/Data/Configurations/` |
| Authentication Endpoints | `src/MyApp.Server.WebApi/Endpoints/AuthEndpoints.cs` |
| Activities | `src/MyApp.Server.Infrastructure/Activities/` |
| Middleware | `src/MyApp.Server.Infrastructure/Middleware/` |
| API Endpoints | `src/MyApp.Server.WebApi/Endpoints/` |
| Validators | `src/MyApp.Server.Application/Validators/` |
| Worker Host | `MyApp.Server.Worker/Program.cs` |
| Aspire Host | `MyApp.AppHost/Program.cs` |
| OpenAPI Document | `src/MyApp.Server.WebApi/MyApp.Server.WebApi.json` |
| Spectral Config | `src/MyApp.Server.WebApi/.spectral.yml` |

### B.3 Troubleshooting

| Issue | Solution |
|-------|----------|
| "Duplicate instance ID" | Client is resubmitting - this is expected idempotency behavior |
| Activity timeout | Increase `MaxConcurrentActivities` or add workers |
| Worker not picking up tasks | Check `durabletask` database connection and TaskHubName |
| Orchestration stuck | Check for non-deterministic code (DateTime.Now, Guid.NewGuid) |
| OAuth redirect fails | Verify redirect URIs in provider console match application settings |
| User not authenticated | Check cookie settings, ensure HTTPS in production |
| EF migrations fail | Ensure `appdb` connection string is correct and database exists (separate from `durabletask`) |
| Wrong database for DTFx | DTFx tables (`dt.*`) must be in `durabletask` database, NOT `appdb` |
| Connection pool exhausted | Check that both `durabletask` and `appdb` connection strings have appropriate pool sizes |

---

*End of Document*
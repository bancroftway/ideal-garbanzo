# MyApp Architecture Plan

> **Document Version:** 2.0  
> **Created:** December 15, 2025  
> **Last Updated:** December 26, 2025  
> **Status:** Architecture Redesign

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Project Goals and Objectives](#2-project-goals-and-objectives)
3. [Technology Stack](#3-technology-stack)
4. [Solution Structure](#4-solution-structure)
5. [Architecture Overview](#5-architecture-overview)
6. [Microsoft Agent Framework Workflows](#6-microsoft-agent-framework-workflows)
7. [Worker Strategy](#7-worker-strategy)
8. [Sample Workflow: Document Ingestion](#8-sample-workflow-document-ingestion)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [AG-UI Protocol & CopilotKit Integration](#10-ag-ui-protocol--copilotkit-integration)
11. [Frontend Authentication & Authorization](#11-frontend-authentication--authorization)
12. [Capacitor Mobile Apps](#12-capacitor-mobile-apps)
13. [API Design](#13-api-design)
14. [Testing Strategy](#14-testing-strategy)
15. [Configuration Management](#15-configuration-management)
16. [Deployment Considerations](#16-deployment-considerations)
17. [Future Enhancements](#17-future-enhancements)

---

## 1. Executive Summary

### 1.1 Purpose

This document describes the architecture and implementation plan for **MyApp**, a self-hosted AI-powered workflow orchestration solution built using the **Microsoft Agent Framework Workflows** with Redis checkpoint persistence. The solution features a React frontend powered by **CopilotKit** for conversational AI interactions, communicating with the backend via the **AG-UI protocol** (Server-Sent Events streaming).

### 1.2 Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Orchestration Framework** | Microsoft Agent Framework Workflows | AI-native workflows, superstep checkpointing, long-running agent support |
| **Application Database** | PostgreSQL | Open-source, robust, excellent JSON support, self-hosted |
| **Workflow State Persistence** | Redis | Fast checkpoint storage, pub/sub for real-time updates, future scaling via Streams |
| **Client-Server Protocol** | AG-UI (SSE) | Real-time streaming, human-in-the-loop, shared state, backend/frontend tools |
| **Web Frontend** | React + CopilotKit v1.50 + Vite | Pre-built AI chat components, generative UI, excellent DX |
| **Mobile Apps** | Capacitor | Share React codebase, native iOS/Android access, single codebase |
| **Hosting Platform** | .NET Aspire | Simplified orchestration, built-in observability, container management |
| **Architecture Pattern** | Clean Architecture | Separation of concerns, testability, maintainability |
| **Validation** | FluentValidation | Declarative rules, auto-validation for endpoints |
| **Observability** | OpenTelemetry | Vendor-neutral, comprehensive tracing and metrics |
| **API Documentation** | OpenAPI 3.1 + Scalar UI | Native .NET 10 support, modern UI, build-time doc generation |

### 1.3 Constraints

- **No Azure cloud dependencies** - The solution must run entirely on self-hosted infrastructure
- **PostgreSQL + Redis** - No Azure Storage, Azure Service Bus, or other Azure-specific backends
- **.NET 10** - Using the latest .NET runtime with preview language features
- **Zero build warnings** - All code must compile without warnings
- **Frontend must be authenticated** - All client apps (web and mobile) must enforce authentication and authorization

---

## 2. Project Goals and Objectives

### 2.1 Primary Goals

1. **AI-Native Workflow Orchestration** - Leverage Microsoft Agent Framework for building intelligent, long-running AI workflows
2. **Real-Time User Experience** - Stream AI responses to users via AG-UI protocol with CopilotKit components
3. **Cross-Platform Clients** - Single React codebase for web (Vite) and mobile (Capacitor) applications
4. **Self-Hosted Solution** - Create a solution that can run entirely on-premises or in any cloud
5. **Production-Ready Patterns** - Implement patterns suitable for production use

### 2.2 Secondary Goals

1. **Comprehensive Observability** - Full tracing and metrics for workflow execution
2. **Clean Architecture** - Demonstrate proper layering and separation of concerns
3. **Robust Validation** - Strong input validation at API boundaries
4. **Checkpoint Recovery** - Support workflow resumption after failures via Redis persistence
5. **Human-in-the-Loop** - Enable user interaction during workflow execution

### 2.3 Non-Goals

- Integration with Azure Functions runtime
- Azure Storage or Service Bus backends
- Multi-tenancy support (deferred to future phases)
- Distributed worker scaling (Phase 2 - Redis Streams)

---

## 3. Technology Stack

### 3.1 Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Runtime framework |
| **C#** | Preview (13+) | Programming language |
| **.NET Aspire** | 13.1.0 | Application orchestration and observability |
| **PostgreSQL** | 16+ | Application database |
| **Redis** | 7+ | Workflow checkpoint storage and caching |

### 3.2 NuGet Packages

#### Microsoft Agent Framework

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI.Workflows` | 1.0.0-preview.* | Workflow orchestration with superstep checkpointing |
| `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` | 1.0.0-preview.* | AG-UI protocol server (SSE streaming) |
| `Microsoft.Extensions.AI` | 9.1.0 | AI abstractions and chat client interfaces |
| `Microsoft.Extensions.AI.OpenAI` | 9.1.0 | OpenAI/Azure OpenAI integration |

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
| `OpenTelemetry.Instrumentation.StackExchangeRedis` | 1.10.0 | Redis instrumentation |

#### API Documentation

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | Native OpenAPI 3.1 document generation |
| `Microsoft.Extensions.ApiDescription.Server` | 10.0.0 | Build-time OpenAPI document generation |
| `Scalar.AspNetCore` | 2.1.9 | Modern interactive API documentation UI |

#### Testing

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.2 | Test framework |
| `Moq` | 4.20.72 | Mocking framework |
| `FluentAssertions` | 6.12.2 | Fluent assertions |

#### Authentication

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.Authentication.Google` | 10.0.0 | Google OAuth provider |
| `Microsoft.AspNetCore.Authentication.Facebook` | 10.0.0 | Facebook OAuth provider |
| `AspNet.Security.OAuth.GitHub` | 10.0.0 | GitHub OAuth provider |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.0 | ASP.NET Core Identity |

#### Entity Framework Core & Aspire

| Package | Version | Purpose |
|---------|---------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.0 | EF Core PostgreSQL provider |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | Design-time EF Core tools |
| `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` | 13.1.0 | Aspire EF Core hosting for PostgreSQL |
| `Aspire.StackExchange.Redis.DistributedCaching` | 13.1.0 | Aspire Redis client integration |
| `Microsoft.Extensions.ServiceDiscovery` | 9.1.0 | Aspire client service discovery |

#### Aspire Hosting

| Package | Version | Purpose |
|---------|---------|---------|
| `Aspire.Hosting.PostgreSQL` | 13.1.0 | PostgreSQL container hosting |
| `Aspire.Hosting.Redis` | 13.1.0 | Redis container hosting |
| `Aspire.Hosting.JavaScript` | 13.1.0 | Vite/React frontend hosting |

### 3.3 Frontend Packages (npm)

#### CopilotKit

| Package | Version | Purpose |
|---------|---------|---------|
| `@copilotkit/react-core` | 1.50.x | Core React hooks and context |
| `@copilotkit/react-ui` | 1.50.x | Pre-built chat UI components |

#### Capacitor (Mobile)

| Package | Version | Purpose |
|---------|---------|---------|
| `@capacitor/core` | 6.x | Core Capacitor runtime |
| `@capacitor/cli` | 6.x | CLI for building mobile apps |
| `@capacitor/android` | 6.x | Android platform support |
| `@capacitor/ios` | 6.x | iOS platform support |
| `@capacitor/browser` | 6.x | In-app browser for OAuth flows |

### 3.4 Development Tools

- **Visual Studio 2022** or **VS Code** with C# Dev Kit
- **Docker Desktop** for containerized PostgreSQL and Redis
- **Aspire Dashboard** for observability
- **Spectral** for OpenAPI document linting
- **Node.js 20+** for frontend development
- **Android Studio** / **Xcode** for mobile development

---

## 4. Solution Structure

### 4.1 Directory Layout

```
MyApp/
├── docs/
│   └── architecture_plan.md
├── src/
│   ├── MyApp.slnx
│   ├── MyApp.AppHost/                    # Aspire orchestration
│   ├── MyApp.ServiceDefaults/            # Shared service configuration
│   ├── MyApp/
│   │   ├── MyApp.Server.Core/            # Domain layer
│   │   ├── MyApp.Server.Application/     # Application layer
│   │   ├── MyApp.Server.Infrastructure/  # Infrastructure layer
│   │   ├── MyApp.Server.WebApi/          # API + AG-UI server
│   │   ├── MyApp.Server.Worker/          # Background worker
│   │   └── MyApp.Shared/                 # Shared DTOs
│   ├── ui/                               # React + CopilotKit frontend
│   │   ├── src/
│   │   ├── public/
│   │   ├── package.json
│   │   ├── vite.config.ts
│   │   └── capacitor.config.ts
│   └── mobile/                           # Capacitor native projects
│       ├── android/
│       └── ios/
└── tests/
    └── MyApp.Tests/
```

### 4.2 Project Responsibilities

| Project | Layer | Responsibility |
|---------|-------|----------------|
| **MyApp.Server.Core** | Domain | Entities, Interfaces, domain types, User entity |
| **MyApp.Server.Application** | Application | Workflow definitions, validators, use cases |
| **MyApp.Server.Infrastructure** | Infrastructure | Activities, middleware, external integrations, DbContext, Redis storage |
| **MyApp.Server.WebApi** | Presentation | HTTP endpoints, AG-UI server, authentication |
| **MyApp.Server.Worker** | Presentation | Background worker, workflow processing |
| **MyApp.AppHost** | Orchestration | Aspire host, resource provisioning |
| **MyApp.ServiceDefaults** | Cross-cutting | OpenTelemetry, health checks, service config |
| **MyApp.Shared** | Shared | DTOs shared between server and clients |
| **MyApp.Tests** | Testing | Unit tests, integration tests |
| **ui/** | Frontend | React + CopilotKit web application |
| **mobile/** | Frontend | Capacitor iOS/Android native shells |

### 4.3 Dependency Flow

```
                    ┌─────────────────┐
                    │  MyApp.AppHost  │
                    │  (Orchestrates) │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
  ┌───────────┐       ┌───────────┐      ┌───────────────┐
  │  WebApi   │       │  Worker   │      │ServiceDefaults│
  │ + AG-UI   │       │           │      └───────────────┘
  └─────┬─────┘       └─────┬─────┘              ▲
        │                   │                    │
        └─────────┬─────────┘                    │
                  │                              │
                  ▼                              │
       ┌─────────────────────┐                   │
       │   Infrastructure    │───────────────────┘
       │  (Redis, Postgres)  │
       └──────────┬──────────┘
                  │
                  ▼
       ┌─────────────────────┐
       │    Application      │
       │   (Workflows)       │
       └──────────┬──────────┘
                  │
                  ▼
       ┌─────────────────────┐
       │       Core          │
       └─────────────────────┘


  ┌─────────────────────────────────────────┐
  │           Frontend Layer                │
  │                                         │
  │  ┌─────────────┐    ┌─────────────────┐ │
  │  │  React +    │    │   Capacitor     │ │
  │  │  CopilotKit │────│  (iOS/Android)  │ │
  │  │  (Vite)     │    │                 │ │
  │  └──────┬──────┘    └─────────────────┘ │
  │         │                               │
  │         │ AG-UI (SSE)                   │
  │         ▼                               │
  │  ┌─────────────┐                        │
  │  │   WebApi    │                        │
  │  └─────────────┘                        │
  └─────────────────────────────────────────┘
```

---

## 5. Architecture Overview

### 5.1 Clean Architecture Principles

The solution follows Clean Architecture (also known as Onion Architecture or Hexagonal Architecture):

1. **Independence of Frameworks** - The core business logic doesn't depend on ASP.NET Core or Agent Framework directly
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
- **Workflows** - Agent Framework workflow definitions
- **Validators** - FluentValidation rules for DTOs
- **Use Cases** - Application-specific business logic

**Dependencies:** Core layer only, plus Agent Framework workflow abstractions.

#### Infrastructure Layer (MyApp.Server.Infrastructure)

Contains:
- **Activities** - Workflow activity implementations
- **Storage** - Redis checkpoint storage implementation
- **External Services** - HTTP clients, AI providers
- **DbContext** - Entity Framework Core PostgreSQL context

**Dependencies:** Core, Application, plus external packages (Agent Framework, Redis, PostgreSQL).

#### Presentation Layer (MyApp.Server.WebApi, MyApp.Server.Worker)

Contains:
- **Endpoints** - HTTP request handlers and AG-UI server
- **Models** - API-specific request/response models
- **Configuration** - Application startup and DI setup

**Dependencies:** All layers.

### 5.3 System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Client Applications                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   React Web     │  │  iOS (Capacitor)│  │Android(Capacitor)│ │
│  │  + CopilotKit   │  │  + CopilotKit   │  │  + CopilotKit   │ │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘ │
│           │                    │                    │           │
│           └────────────────────┼────────────────────┘           │
│                                │                                 │
│                          AG-UI (SSE)                             │
└────────────────────────────────┼─────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                          WebApi Server                           │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  AG-UI Endpoint (/api/agent)                            │    │
│  │  - Server-Sent Events streaming                         │    │
│  │  - Backend tools execution                              │    │
│  │  - Human-in-the-loop support                            │    │
│  └─────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  REST Endpoints                                         │    │
│  │  - Authentication (OAuth)                               │    │
│  │  - Document management                                  │    │
│  │  - Workflow status                                      │    │
│  └─────────────────────────────────────────────────────────┘    │
└────────────────────────────────┬────────────────────────────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
              ▼                  ▼                  ▼
       ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
       │ PostgreSQL  │    │    Redis    │    │   Worker    │
       │   (appdb)   │    │(checkpoints)│    │ (workflows) │
       │             │    │             │    │             │
       │ - Users     │    │ - State     │    │ - Executor  │
       │ - Documents │    │ - Cache     │    │ - Activities│
       │ - Identity  │    │ - Pub/Sub   │    │             │
       └─────────────┘    └─────────────┘    └─────────────┘
```

---

## 6. Microsoft Agent Framework Workflows

### 6.1 What is Microsoft Agent Framework?

The **Microsoft Agent Framework** is a modern framework for building AI-native workflow orchestrations in .NET. It provides superstep-based checkpointing, allowing long-running AI workflows to persist state and resume after failures.

### 6.2 Core Concepts

#### Workflows

- **Definition:** A workflow built using `WorkflowBuilder` that defines a sequence of steps
- **Execution Model:** Superstep-based checkpointing at defined boundaries
- **State:** Persisted via `IStorage` interface (Redis in our implementation)
- **Recovery:** Explicit resumption via `ResumeStreamAsync()`

```csharp
public static class DocumentIngestionWorkflow
{
    public static WorkflowBuilder<DocumentInput, DocumentOutput> Create()
    {
        return new WorkflowBuilder<DocumentInput, DocumentOutput>()
            .AddStep("extract", ExtractContent)
            .AddStep("analyze", AnalyzeWithAI)
            .AddStep("summarize", GenerateSummary)
            .WithCheckpoint("after-extraction")
            .WithCheckpoint("after-analysis");
    }
}
```

#### Activities

- **Definition:** Individual units of work within a workflow
- **Execution Model:** Can be async, support cancellation
- **Side Effects:** Activities CAN have side effects (database writes, HTTP calls, AI inference)
- **Idempotency:** Activities SHOULD be idempotent for retry safety

```csharp
public class ExtractContentActivity
{
    public async Task<ExtractionResult> ExecuteAsync(
        DocumentInput input, 
        CancellationToken cancellationToken)
    {
        // Perform extraction work
        return new ExtractionResult { Content = extractedContent };
    }
}
```

#### Checkpointing

- **Purpose:** Save workflow state at defined boundaries (supersteps)
- **Granularity:** Coarser than DTFx event-level replay
- **Storage:** Custom `IStorage` implementation (Redis)
- **Recovery:** Call `ResumeStreamAsync()` with checkpoint ID

```csharp
// Checkpoint storage interface
public interface IStorage
{
    Task<IDictionary<string, object>> ReadAsync(
        string[] keys, 
        CancellationToken cancellationToken);
    
    Task WriteAsync<TStoreItem>(
        IDictionary<string, TStoreItem> changes, 
        CancellationToken cancellationToken);
    
    Task DeleteAsync(
        string[] keys, 
        CancellationToken cancellationToken);
}
```

### 6.3 Redis Checkpoint Storage

**Custom implementation required** - No pre-built Redis storage exists:

```csharp
public class RedisCheckpointStorage : IStorage
{
    private readonly IConnectionMultiplexer _redis;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Key pattern: workflow:{workflowId}:checkpoint:{checkpointId}
    private const string KeyPattern = "workflow:{0}:checkpoint:{1}";
    
    public async Task<IDictionary<string, object>> ReadAsync(
        string[] keys, 
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var result = new Dictionary<string, object>();
        
        foreach (var key in keys)
        {
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
            {
                result[key] = JsonSerializer.Deserialize<object>(
                    value!, _jsonOptions)!;
            }
        }
        
        return result;
    }
    
    public async Task WriteAsync<TStoreItem>(
        IDictionary<string, TStoreItem> changes, 
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var batch = db.CreateBatch();
        
        foreach (var (key, value) in changes)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            _ = batch.StringSetAsync(key, json, TimeSpan.FromDays(7));
        }
        
        batch.Execute();
    }
    
    public async Task DeleteAsync(
        string[] keys, 
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        await db.KeyDeleteAsync(redisKeys);
    }
}
```

### 6.4 Workflow Execution

```csharp
// Starting a workflow
var executor = new WorkflowExecutor(storage, activityRegistry);
var workflowId = Guid.NewGuid().ToString();

await foreach (var update in executor.RunAsync(workflow, input, workflowId))
{
    // Stream updates to client via AG-UI
    yield return update;
}

// Resuming from checkpoint after failure
await foreach (var update in executor.ResumeStreamAsync(workflowId, checkpointId))
{
    yield return update;
}
```

### 6.5 Comparison: Agent Framework vs DTFx

| Feature | Agent Framework | DTFx |
|---------|-----------------|------|
| **Checkpointing** | Superstep-level | Event-level (every await) |
| **Recovery** | Explicit via `ResumeStreamAsync` | Automatic deterministic replay |
| **State Storage** | Custom `IStorage` (Redis) | SQL Server tables |
| **AI Integration** | Native (Microsoft.Extensions.AI) | Manual integration |
| **Streaming** | AG-UI protocol (SSE) | Polling-based |
| **Human-in-Loop** | Built-in support | External events |
| **Distributed** | Single worker (Phase 1) | Multiple workers polling |

---

## 7. Worker Strategy

### 7.1 Single Worker Architecture (Phase 1)

The Microsoft Agent Framework uses in-memory task queues (`ActivityTaskQueue`, `HostedActivityService`), which means **a single worker process** handles all workflow execution in Phase 1.

```
┌─────────────────────────────────────────────────────────────┐
│                     Single Worker Process                    │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  WorkflowExecutor                                   │   │
│  │  - Runs workflows                                   │   │
│  │  - Manages checkpoints                              │   │
│  │  - Streams via AG-UI                                │   │
│  └───────────────────────┬─────────────────────────────┘   │
│                          │                                  │
│  ┌───────────────────────▼─────────────────────────────┐   │
│  │  ActivityTaskQueue (In-Memory)                      │   │
│  │  - Queues activity tasks                            │   │
│  │  - Single-process only                              │   │
│  └───────────────────────┬─────────────────────────────┘   │
│                          │                                  │
│  ┌───────────────────────▼─────────────────────────────┐   │
│  │  HostedActivityService                              │   │
│  │  - Executes activities                              │   │
│  │  - Background processing                            │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                   ┌─────────────┐
                   │    Redis    │
                   │ Checkpoints │
                   └─────────────┘
```

### 7.2 Aspire Configuration (Phase 1)

```csharp
// Program.cs in MyApp.AppHost
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL for application data
var postgres = builder.AddPostgres("postgres")
    .AddDatabase("appdb");

// Redis for workflow checkpoints
var redis = builder.AddRedis("redis");

// Single worker (no replicas in Phase 1)
var worker = builder.AddProject<Projects.MyApp_Server_Worker>("worker")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

// WebApi with AG-UI server
var webapi = builder.AddProject<Projects.MyApp_Server_WebApi>("webapi")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(worker)
    .WaitFor(postgres)
    .WaitFor(redis);

// React + CopilotKit frontend
builder.AddViteApp("frontend", "../ui")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithReference(webapi)
    .WaitFor(webapi);

builder.Build().Run();
```

### 7.3 Phase 2: Distributed Workers with Redis Streams

For horizontal scaling, replace in-memory queues with Redis Streams:

```
┌─────────────────────────────────────────────────────────────┐
│                    Redis Streams (Phase 2)                   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  workflow:tasks Stream                              │   │
│  │  - Distributed task queue                           │   │
│  │  - Consumer groups                                  │   │
│  │  - Automatic rebalancing                            │   │
│  └───────────────────────┬─────────────────────────────┘   │
│                          │                                  │
│      ┌───────────────────┼───────────────────┐             │
│      │                   │                   │             │
│      ▼                   ▼                   ▼             │
│  ┌────────┐         ┌────────┐         ┌────────┐         │
│  │Worker 1│         │Worker 2│         │Worker 3│         │
│  │Consumer│         │Consumer│         │Consumer│         │
│  │Group   │         │Group   │         │Group   │         │
│  └────────┘         └────────┘         └────────┘         │
└─────────────────────────────────────────────────────────────┘
```

**Phase 2 Implementation:**
```csharp
// Redis Streams-based task queue
public class RedisActivityTaskQueue : IActivityTaskQueue
{
    private readonly IConnectionMultiplexer _redis;
    private const string StreamKey = "workflow:tasks";
    private const string ConsumerGroup = "workers";
    
    public async Task EnqueueAsync(ActivityTask task)
    {
        var db = _redis.GetDatabase();
        await db.StreamAddAsync(StreamKey, [
            new NameValueEntry("payload", JsonSerializer.Serialize(task))
        ]);
    }
    
    public async Task<ActivityTask?> DequeueAsync(string consumerId)
    {
        var db = _redis.GetDatabase();
        var entries = await db.StreamReadGroupAsync(
            StreamKey, ConsumerGroup, consumerId, count: 1);
        // Process and acknowledge...
    }
}
```

### 7.4 Scaling Considerations

| Phase | Workers | Queue Type | Use Case |
|-------|---------|------------|----------|
| **Phase 1** | 1 | In-memory | Development, low-volume |
| **Phase 2** | N | Redis Streams | Production, high-volume |

**When to Scale:**
- CPU-bound workflows: Add more workers
- I/O-bound workflows: Increase concurrency per worker
- Long-running AI: Consider dedicated worker pools

---

## 8. Sample Workflow: Document Ingestion

### 8.1 Workflow Description

The **IngestDocument** workflow demonstrates a fan-out/fan-in pattern with AI integration:

1. **Receive** document file via HTTP POST
2. **Fan-out** to 3 parallel activities (Docling, MarkitDown, Marker)
3. **Fan-in** results to AI summarization activity
4. **Stream** results to client via AG-UI
5. **Return** final summary

### 8.2 Workflow Diagram

```
                    ┌──────────────────┐
                    │  AG-UI Client    │
                    │  (CopilotKit)    │
                    └────────┬─────────┘
                             │ SSE Stream
                             ▼
                    ┌──────────────────┐
                    │  /api/agent      │
                    │  AG-UI Endpoint  │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │ IngestDocument   │
                    │    Workflow      │
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
                    │   AI Summarize   │
                    │    Activity      │
                    │ (LLM via M.E.AI) │
                    └────────┬─────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │  Stream Result   │
                    │   via AG-UI      │
                    └──────────────────┘
```

### 8.3 Implementation Details

#### Workflow Definition

```csharp
public static class DocumentIngestionWorkflow
{
    public static WorkflowBuilder<DocumentInput, DocumentOutput> Create(
        IChatClient chatClient)
    {
        return new WorkflowBuilder<DocumentInput, DocumentOutput>()
            .AddStep("extract-parallel", async (context, input) =>
            {
                // Fan-out: Execute all extractors in parallel
                var tasks = new[]
                {
                    context.RunActivityAsync<string>("docling", input.FileContent),
                    context.RunActivityAsync<string>("markitdown", input.FileContent),
                    context.RunActivityAsync<string>("marker", input.FileContent)
                };
                
                var results = await Task.WhenAll(tasks);
                return new ExtractionResults
                {
                    DoclingResult = results[0],
                    MarkitDownResult = results[1],
                    MarkerResult = results[2]
                };
            })
            .WithCheckpoint("after-extraction")
            .AddStep("summarize", async (context, extraction) =>
            {
                // Fan-in: AI summarization
                var prompt = $"""
                    Summarize these document extractions:
                    
                    Docling: {extraction.DoclingResult}
                    MarkitDown: {extraction.MarkitDownResult}
                    Marker: {extraction.MarkerResult}
                    """;
                
                var response = await chatClient.CompleteAsync(prompt);
                return new DocumentOutput
                {
                    Summary = response.Message.Text,
                    ProcessedAt = DateTime.UtcNow
                };
            })
            .WithCheckpoint("completed");
    }
}
```

### 8.4 Activity Implementations

Each activity performs document processing:

| Activity | Input | Output | Purpose |
|----------|-------|--------|---------|
| `DoclingActivity` | Base64 file | Markdown | Convert using IBM Docling |
| `MarkitDownActivity` | Base64 file | Markdown | Convert using Microsoft MarkitDown |
| `MarkerActivity` | Base64 file | Markdown | OCR using Marker library |
| `SummarizeActivity` | 3 markdown texts | Summary | AI summarization via M.E.AI |

---

## 9. Cross-Cutting Concerns

The Microsoft Agent Framework provides native middleware support for implementing cross-cutting concerns. There are distinct middleware systems for different layers: **turn-level middleware** for agent interactions, **callback middleware** for AI agent invocations, and **manual instrumentation** for workflow steps.

### 9.1 Turn-Level Middleware (IMiddleware)

The `IMiddleware` interface intercepts all incoming activities (messages, events) to an agent. This is the primary extensibility point for cross-cutting concerns at the agent level.

#### Middleware Interface

```csharp
public interface IMiddleware
{
    Task OnTurnAsync(
        ITurnContext turnContext,
        NextDelegate next,
        CancellationToken cancellationToken);
}
```

#### Custom Middleware Implementation

```csharp
public class TelemetryMiddleware : IMiddleware
{
    private readonly ActivitySource _source;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(ILogger<TelemetryMiddleware> logger)
    {
        _source = new ActivitySource("MyApp.Agent");
        _logger = logger;
    }

    public async Task OnTurnAsync(
        ITurnContext turnContext,
        NextDelegate next,
        CancellationToken cancellationToken)
    {
        using var activity = _source.StartActivity("agent.turn");
        activity?.SetTag("activity.type", turnContext.Activity.Type);
        activity?.SetTag("activity.id", turnContext.Activity.Id);

        _logger.LogInformation("Agent turn started: {ActivityType}", 
            turnContext.Activity.Type);
        var sw = Stopwatch.StartNew();

        try
        {
            await next(cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Agent turn failed");
            throw;
        }
        finally
        {
            _logger.LogInformation("Agent turn completed in {Elapsed}ms", 
                sw.ElapsedMilliseconds);
        }
    }
}
```

#### Middleware Registration

```csharp
// Program.cs - Register middleware array via DI
builder.Services.AddSingleton<IMiddleware[]>([
    new TelemetryMiddleware(logger),
    new TranscriptLoggerMiddleware(new FileTranscriptLogger()),
    new ValidationMiddleware()
]);
```

### 9.2 Agent Callback Middleware (CallbackMiddleware)

For AI agent-specific interception (LLM calls, tool invocations), use `CallbackMiddleware<TContext>`. This provides fine-grained control over agent behavior including PII filtering, guardrails, and function call interception.

#### Available Callback Contexts

| Context Type | Purpose |
|--------------|---------|
| `AgentInvokeCallbackContext` | Intercept agent invocations (input/output filtering) |
| `AgentFunctionInvocationCallbackContext` | Intercept tool/function calls |

#### PII Filtering Middleware

```csharp
public class PiiFilteringMiddleware : CallbackMiddleware<AgentInvokeCallbackContext>
{
    private static readonly string[] PiiPatterns = ["email", "phone", "ssn", "credit"];

    public override async Task OnProcessAsync(
        AgentInvokeCallbackContext context,
        Func<AgentInvokeCallbackContext, Task> next,
        CancellationToken cancellationToken)
    {
        // Pre-processing: Filter input messages for PII
        context.Messages = context.Messages
            .Select(m => new ChatMessage(m.Role, RedactPii(m.Text)))
            .ToList();

        await next(context);

        // Post-processing: Filter output for PII (non-streaming)
        if (!context.IsStreaming)
        {
            context.Messages = context.Messages
                .Select(m => new ChatMessage(m.Role, RedactPii(m.Text)))
                .ToList();
        }
    }

    private static string RedactPii(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        // Implement PII detection and redaction logic
        return text;
    }
}
```

#### Guardrail Middleware

```csharp
public class GuardrailMiddleware : CallbackMiddleware<AgentInvokeCallbackContext>
{
    private readonly string[] _forbiddenKeywords = ["harmful", "illegal", "violence"];

    public override async Task OnProcessAsync(
        AgentInvokeCallbackContext context,
        Func<AgentInvokeCallbackContext, Task> next,
        CancellationToken cancellationToken)
    {
        // Check input for forbidden content
        foreach (var message in context.Messages)
        {
            if (_forbiddenKeywords.Any(k => 
                message.Text?.Contains(k, StringComparison.OrdinalIgnoreCase) == true))
            {
                context.Messages.Add(new ChatMessage(Role.Assistant,
                    "I cannot process this request due to content policy."));
                return; // Short-circuit the pipeline
            }
        }

        await next(context);

        // For streaming responses, wrap the stream with guardrail filtering
        if (context.IsStreaming)
        {
            context.SetRawResponse(FilterStreamAsync(context.RunStreamingResponse!));
        }
    }
}
```

#### Function Call Interception

```csharp
public class FunctionCallLoggingMiddleware 
    : CallbackMiddleware<AgentFunctionInvocationCallbackContext>
{
    private readonly ILogger _logger;

    public override async Task OnProcessAsync(
        AgentFunctionInvocationCallbackContext context,
        Func<AgentFunctionInvocationCallbackContext, Task> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Function call: {FunctionName}, Streaming: {IsStreaming}",
            context.FunctionName, context.IsStreaming);

        var sw = Stopwatch.StartNew();
        
        try
        {
            await next(context);
            _logger.LogInformation("Function {FunctionName} completed in {Elapsed}ms",
                context.FunctionName, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Function {FunctionName} failed", context.FunctionName);
            throw;
        }
    }
}
```

#### Callback Middleware Registration

```csharp
// Using the fluent builder pattern
var agent = chatClient.CreateAIAgent(model)
    .AsBuilder()
    .UseCallbacks(config =>
    {
        config.AddCallback(new PiiFilteringMiddleware());
        config.AddCallback(new GuardrailMiddleware());
        config.AddCallback(new FunctionCallLoggingMiddleware(logger));
    })
    .Build();

// Or using inline delegates
var agent = chatClient.CreateAIAgent(model)
    .AsBuilder()
    .Use(runFunc: async (context, next) =>
    {
        Console.WriteLine("Before agent run");
        await next(context);
        Console.WriteLine("After agent run");
    })
    .Build();
```

### 9.3 Built-in TranscriptLoggerMiddleware

The Agent Framework includes `TranscriptLoggerMiddleware` for logging all conversations to persistent storage. This is useful for auditing, debugging, and compliance.

#### File-Based Transcript Logging

```csharp
// Register transcript logging middleware
builder.Services.AddSingleton<IMiddleware[]>([
    new TranscriptLoggerMiddleware(new FileTranscriptLogger())
]);
```

#### Custom Transcript Logger

```csharp
public class DatabaseTranscriptLogger : ITranscriptLogger
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseTranscriptLogger(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogActivityAsync(IActivity activity)
    {
        var transcript = new ConversationTranscript
        {
            ActivityId = activity.Id,
            ConversationId = activity.Conversation.Id,
            Type = activity.Type,
            Timestamp = activity.Timestamp ?? DateTimeOffset.UtcNow,
            FromId = activity.From?.Id,
            Text = activity is IMessageActivity msg ? msg.Text : null,
            RawJson = JsonSerializer.Serialize(activity)
        };

        _dbContext.Transcripts.Add(transcript);
        await _dbContext.SaveChangesAsync();
    }
}

// Registration
builder.Services.AddScoped<ITranscriptLogger, DatabaseTranscriptLogger>();
builder.Services.AddSingleton<IMiddleware[]>(sp => [
    new TranscriptLoggerMiddleware(sp.GetRequiredService<ITranscriptLogger>())
]);
```

### 9.4 Centralized Error Handling

The `AgentApplication` class provides centralized error handling via the `OnError` handler. This catches all unhandled exceptions and allows graceful error responses.

#### Error Handler Registration

```csharp
public class MyAgent : AgentApplication
{
    public MyAgent(AgentApplicationOptions options) : base(options)
    {
        // Register error handler
        OnError(HandleErrorAsync);

        // Register activity handlers
        OnActivity(ActivityTypes.Message, OnMessageAsync, rank: RouteRank.Last);
    }

    private async Task HandleErrorAsync(
        ITurnContext turnContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the error with full context
        _logger.LogError(exception, 
            "Unhandled error in agent. ConversationId: {ConversationId}, ActivityType: {ActivityType}",
            turnContext.Activity.Conversation.Id,
            turnContext.Activity.Type);

        // Record error in OpenTelemetry
        Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
        Activity.Current?.RecordException(exception);

        // Send graceful error message to user
        await turnContext.SendActivityAsync(
            MessageFactory.Text("I encountered an error processing your request. Please try again."),
            cancellationToken);

        // Optionally: Send to dead letter queue for analysis
        await _deadLetterQueue.EnqueueAsync(new ErrorRecord
        {
            ConversationId = turnContext.Activity.Conversation.Id,
            Exception = exception.ToString(),
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
```

#### Exception-Specific Handling

```csharp
private async Task HandleErrorAsync(
    ITurnContext turnContext,
    Exception exception,
    CancellationToken cancellationToken)
{
    var userMessage = exception switch
    {
        RateLimitException rle => 
            $"Service is busy. Please try again in {rle.RetryAfter.TotalSeconds} seconds.",
        ValidationException ve => 
            $"Invalid input: {ve.Message}",
        TimeoutException => 
            "The request timed out. Please try again.",
        _ => 
            "An unexpected error occurred. Please try again later."
    };

    await turnContext.SendActivityAsync(
        MessageFactory.Text(userMessage),
        cancellationToken);
}
```

### 9.5 Workflow Step Instrumentation

Workflow steps (activities) in Microsoft Agent Framework Workflows do **not** have built-in middleware. Instrumentation must be done manually within each workflow step using OpenTelemetry.

#### Manual Step Instrumentation Pattern

```csharp
public static class DocumentIngestionWorkflow
{
    private static readonly ActivitySource Source = new("MyApp.Workflows");

    public static WorkflowBuilder<DocumentInput, DocumentOutput> Create(
        IChatClient chatClient)
    {
        return new WorkflowBuilder<DocumentInput, DocumentOutput>()
            .AddStep("extract-parallel", async (context, input) =>
            {
                using var activity = Source.StartActivity("workflow.step.extract");
                activity?.SetTag("input.file_size", input.FileContent.Length);
                activity?.SetTag("input.repository", input.RepositoryName);

                try
                {
                    var tasks = new[]
                    {
                        context.RunActivityAsync<string>("docling", input.FileContent),
                        context.RunActivityAsync<string>("markitdown", input.FileContent),
                        context.RunActivityAsync<string>("marker", input.FileContent)
                    };

                    var results = await Task.WhenAll(tasks);
                    
                    activity?.SetTag("output.result_count", results.Length);
                    activity?.SetStatus(ActivityStatusCode.Ok);

                    return new ExtractionResults
                    {
                        DoclingResult = results[0],
                        MarkitDownResult = results[1],
                        MarkerResult = results[2]
                    };
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.RecordException(ex);
                    throw;
                }
            })
            .WithCheckpoint("after-extraction")
            .AddStep("summarize", async (context, extraction) =>
            {
                using var activity = Source.StartActivity("workflow.step.summarize");
                
                // ... implementation with instrumentation
            });
    }
}
```

#### Reusable Instrumentation Helper

```csharp
public static class WorkflowInstrumentation
{
    private static readonly ActivitySource Source = new("MyApp.Workflows");

    public static async Task<TResult> ExecuteStepAsync<TInput, TResult>(
        string stepName,
        TInput input,
        Func<Task<TResult>> execution,
        Action<Activity, TInput>? enrichInput = null,
        Action<Activity, TResult>? enrichOutput = null)
    {
        using var activity = Source.StartActivity($"workflow.step.{stepName}");
        
        enrichInput?.Invoke(activity!, input);
        RecordInput(activity!, input);

        try
        {
            var result = await execution();
            
            enrichOutput?.Invoke(activity!, result);
            RecordOutput(activity!, result);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }

    private static void RecordInput<T>(Activity activity, T input)
    {
        var json = JsonSerializer.Serialize(input);
        activity.AddEvent(new ActivityEvent("input.recorded", tags: new ActivityTagsCollection
        {
            ["input.type"] = typeof(T).Name,
            ["input.size"] = json.Length,
            ["input.preview"] = Truncate(json, 1024)
        }));
    }

    private static void RecordOutput<T>(Activity activity, T output)
    {
        var json = JsonSerializer.Serialize(output);
        activity.AddEvent(new ActivityEvent("output.recorded", tags: new ActivityTagsCollection
        {
            ["output.type"] = typeof(T).Name,
            ["output.size"] = json.Length
        }));
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
```

#### Usage in Workflow Steps

```csharp
.AddStep("extract", async (context, input) =>
{
    return await WorkflowInstrumentation.ExecuteStepAsync(
        "extract",
        input,
        async () =>
        {
            // Actual extraction logic
            var results = await Task.WhenAll(/* ... */);
            return new ExtractionResults { /* ... */ };
        },
        enrichInput: (activity, inp) => 
            activity.SetTag("file.size", inp.FileContent.Length),
        enrichOutput: (activity, result) => 
            activity.SetTag("extraction.count", 3));
})
```

### 9.6 OpenTelemetry Integration

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
| Agent turn | `agent.turn` |
| Workflow step | `workflow.step.{name}` |
| Activity execution | `workflow.activity.{name}` |
| AG-UI stream | `agui.stream` |
| Function call | `agent.function.{name}` |

#### Metric Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| Counter | Plural noun | `workflow.executions` |
| Histogram | `.duration` suffix | `workflow.step.duration` |
| UpDownCounter | Singular noun | `workflow.active` |

#### OpenTelemetry Configuration

```csharp
// ServiceDefaults/Extensions.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("MyApp.Agent")
            .AddSource("MyApp.Workflows")
            .AddSource("Microsoft.Agents.AI.*")
            .AddSource("Microsoft.Extensions.AI.*")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRedisInstrumentation();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("MyApp.Workflows")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });
```

### 9.7 Input/Output Recording

**Captured as span events** for proper timestamping:

```csharp
public static void RecordInput<T>(Activity activity, T input) where T : class
{
    var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
    
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

### 9.8 Performance Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `agent.turns` | Counter | Total agent turns processed |
| `agent.turn.duration` | Histogram | Agent turn duration in seconds |
| `workflow.executions` | Counter | Total workflows started |
| `workflow.step.duration` | Histogram | Workflow step duration in seconds |
| `workflow.errors` | Counter | Total errors (tagged by type) |
| `agui.connections` | UpDownCounter | Active AG-UI connections |
| `function.calls` | Counter | Total function/tool calls |

### 9.9 Entity Framework Core

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
            
        // PostgreSQL-specific: Use jsonb for metadata
        builder.Property(d => d.Metadata)
            .HasColumnType("jsonb");
    }
}
```

#### Aspire EF Core Integration

**WebApi (Program.cs):**
```csharp
// Use Aspire EF Core hosting package for PostgreSQL
builder.AddNpgsqlDbContext<ApplicationDbContext>("appdb", 
    configureDbContextOptions: options =>
    {
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    });
```

**Required NuGet Packages:**
- `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` (13.1.0) - Hosting integration
- `Microsoft.Extensions.ServiceDiscovery` (9.1.0) - Service discovery client

**Migration Commands:**
```bash
# Add migration
dotnet ef migrations add InitialCreate --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi

# Update database
dotnet ef database update --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi
```

---

## 10. AG-UI Protocol & CopilotKit Integration

### 10.1 What is AG-UI?

The **AG-UI (Agent User Interaction) Protocol** is a standardized communication protocol between AI agents and user interfaces. It uses Server-Sent Events (SSE) for real-time streaming of AI responses.

### 10.2 AG-UI Server Setup

**WebApi Configuration:**
```csharp
// Program.cs
builder.Services.AddAGUI();

// Configure the AI agent
var chatClient = builder.Services
    .AddOpenAIClient(builder.Configuration["OpenAI:ApiKey"])
    .AsIChatClient();

var tools = [
    AIFunctionFactory.Create(documentService.IngestDocumentAsync),
    AIFunctionFactory.Create(documentService.GetDocumentStatusAsync),
];

ChatClientAgent agent = chatClient.CreateAIAgent(
    name: "DocumentAssistant",
    instructions: "You are a helpful assistant for document processing...",
    tools: tools);

// Map the AG-UI endpoint
app.MapAGUI("/api/agent", agent);
```

### 10.3 AG-UI Protocol Features

| Feature | Description |
|---------|-------------|
| **Agentic Chat** | Real-time AI conversation streaming |
| **Backend Tools** | Server-side function execution |
| **Frontend Tools** | Client-side function execution |
| **Human-in-the-Loop** | User confirmation for sensitive actions |
| **Shared State** | Bidirectional state synchronization |
| **Generative UI** | Dynamic UI component rendering |

### 10.4 CopilotKit React Integration

#### Installation

```bash
cd ui
npm install @copilotkit/react-core@1.50 @copilotkit/react-ui@1.50
```

#### Provider Setup

**App.tsx:**
```tsx
import { CopilotKit } from "@copilotkit/react-core";
import { CopilotSidebar } from "@copilotkit/react-ui";
import "@copilotkit/react-ui/styles.css";

function App() {
  const { token } = useAuth(); // Your auth hook
  
  return (
    <CopilotKit 
      runtimeUrl="/api/agent"
      headers={{ Authorization: `Bearer ${token}` }}
    >
      <YourApp />
      <CopilotSidebar />
    </CopilotKit>
  );
}
```

#### Available UI Components

| Component | Purpose |
|-----------|---------|
| `<CopilotSidebar>` | Collapsible side panel chat |
| `<CopilotPopup>` | Floating popup chat |
| `<CopilotChat>` | Inline chat component |
| `<CopilotTextarea>` | AI-enhanced text input |

#### Frontend Tools (Client-Side Actions)

```tsx
import { useCopilotAction, useCopilotReadable } from "@copilotkit/react-core";

function DocumentViewer() {
  const [documents, setDocuments] = useState<Document[]>([]);
  
  // Expose state to the AI
  useCopilotReadable({
    description: "List of user's documents",
    value: documents,
  });
  
  // Define client-side action
  useCopilotAction({
    name: "highlightDocument",
    description: "Highlight a specific document in the list",
    parameters: [
      { name: "documentId", type: "string", required: true }
    ],
    handler: async ({ documentId }) => {
      // Client-side UI action
      highlightDocumentInUI(documentId);
      return `Highlighted document ${documentId}`;
    },
  });
  
  return <DocumentList documents={documents} />;
}
```

### 10.5 Backend Tools (Server-Side Actions)

```csharp
// DocumentService.cs
public class DocumentService
{
    [Description("Ingest a document for processing")]
    public async Task<IngestResult> IngestDocumentAsync(
        [Description("The document file content")] string fileContent,
        [Description("Target repository name")] string repositoryName,
        CancellationToken cancellationToken)
    {
        // Execute workflow, return result
        var workflowId = await _workflowExecutor.StartAsync(
            DocumentIngestionWorkflow.Create(_chatClient),
            new DocumentInput { FileContent = fileContent, RepositoryName = repositoryName });
        
        return new IngestResult { WorkflowId = workflowId };
    }
}
```

### 10.6 Human-in-the-Loop

```csharp
// Backend tool requiring confirmation
[Description("Delete a document permanently")]
[RequiresConfirmation("Are you sure you want to delete this document?")]
public async Task<DeleteResult> DeleteDocumentAsync(
    [Description("Document ID to delete")] string documentId,
    CancellationToken cancellationToken)
{
    await _documentRepository.DeleteAsync(documentId);
    return new DeleteResult { Success = true };
}
```

---

## 11. Frontend Authentication & Authorization

### 11.1 Authentication Architecture

**CRITICAL:** All frontend applications (web and mobile) MUST enforce authentication. The AG-UI endpoint and all protected API endpoints require valid authentication tokens.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Authentication Flow                          │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐ │
│  │ React App   │───>│  /api/auth  │───>│ OAuth Provider      │ │
│  │ (CopilotKit)│    │  /login     │    │ (Google/FB/GitHub)  │ │
│  └─────────────┘    └─────────────┘    └─────────────────────┘ │
│         │                                        │              │
│         │           ┌────────────────────────────┘              │
│         │           │                                           │
│         │           ▼                                           │
│         │    ┌─────────────┐                                    │
│         │    │  Callback   │                                    │
│         │    │  + Cookie   │                                    │
│         │    └──────┬──────┘                                    │
│         │           │                                           │
│         ▼           ▼                                           │
│  ┌──────────────────────────────┐                               │
│  │  Authenticated Requests      │                               │
│  │  - AG-UI: /api/agent         │                               │
│  │  - REST: /api/documents      │                               │
│  └──────────────────────────────┘                               │
└─────────────────────────────────────────────────────────────────┘
```

### 11.2 WebApi Authentication Configuration

```csharp
// Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
});

builder.Services.AddAuthorization();

// Protect AG-UI endpoint
app.MapAGUI("/api/agent", agent).RequireAuthorization();
```

### 11.3 React Authentication Implementation

#### Auth Context

```tsx
// src/contexts/AuthContext.tsx
import { createContext, useContext, useState, useEffect } from 'react';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (provider: 'google' | 'facebook' | 'github') => void;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check authentication status on mount
    fetch('/api/auth/user', { credentials: 'include' })
      .then(res => res.ok ? res.json() : null)
      .then(data => {
        setUser(data?.isAuthenticated ? data : null);
        setIsLoading(false);
      });
  }, []);

  const login = (provider: 'google' | 'facebook' | 'github') => {
    window.location.href = `/api/auth/login/${provider}`;
  };

  const logout = async () => {
    await fetch('/api/auth/logout', { 
      method: 'POST', 
      credentials: 'include' 
    });
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ 
      user, 
      isAuthenticated: !!user, 
      isLoading, 
      login, 
      logout 
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext)!;
```

#### Protected Routes

```tsx
// src/components/ProtectedRoute.tsx
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
```

#### App with Authentication

```tsx
// src/App.tsx
import { CopilotKit } from "@copilotkit/react-core";
import { CopilotSidebar } from "@copilotkit/react-ui";
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';

function AuthenticatedApp() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return (
    <CopilotKit runtimeUrl="/api/agent">
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route 
          path="/documents" 
          element={
            <ProtectedRoute>
              <DocumentsPage />
            </ProtectedRoute>
          } 
        />
      </Routes>
      <CopilotSidebar />
    </CopilotKit>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AuthenticatedApp />
    </AuthProvider>
  );
}
```

### 11.4 Authentication Endpoints

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/api/auth/login/{provider}` | GET | No | Initiate OAuth flow |
| `/api/auth/callback` | GET | No | OAuth callback handler |
| `/api/auth/logout` | POST | Yes | Sign out user |
| `/api/auth/user` | GET | No | Get current user info |
| `/api/agent` | POST | **Yes** | AG-UI streaming endpoint |
| `/api/documents/*` | ALL | **Yes** | Document management |

### 11.5 Security Best Practices

| Practice | Implementation |
|----------|----------------|
| **HTTPS Only** | All auth flows over TLS |
| **HttpOnly Cookies** | Prevent XSS token theft |
| **SameSite=Strict** | CSRF protection |
| **Secure Flag** | Cookie only sent over HTTPS |
| **Short Token Expiry** | 7 days with refresh |
| **CORS Configuration** | Restrict to known origins |

```csharp
// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://myapp.com")
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## 12. Capacitor Mobile Apps

### 12.1 Overview

**Capacitor** enables building native iOS and Android apps using the same React + CopilotKit codebase. The mobile apps share the web frontend code while gaining access to native device features.

### 12.2 Project Setup

```bash
# Initialize Capacitor in the ui project
cd ui
npm install @capacitor/core @capacitor/cli
npx cap init MyApp com.myapp.app

# Add native platforms
npm install @capacitor/android @capacitor/ios
npx cap add android
npx cap add ios

# Install required plugins
npm install @capacitor/browser  # For OAuth flows
```

### 12.3 Capacitor Configuration

**capacitor.config.ts:**
```typescript
import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.myapp.app',
  appName: 'MyApp',
  webDir: 'dist',
  server: {
    // Development: use local server
    url: 'http://10.0.2.2:3000', // Android emulator
    // url: 'http://localhost:3000', // iOS simulator
    cleartext: true, // Only for development
    
    // Production: use bundled assets
    // androidScheme: 'https',
  },
  plugins: {
    Browser: {
      // OAuth redirect handling
    }
  }
};

export default config;
```

### 12.4 Mobile Authentication

Mobile apps use in-app browser for OAuth flows:

```tsx
// src/hooks/useMobileAuth.ts
import { Browser } from '@capacitor/browser';
import { App } from '@capacitor/app';
import { Capacitor } from '@capacitor/core';

export function useMobileAuth() {
  const login = async (provider: 'google' | 'facebook' | 'github') => {
    if (Capacitor.isNativePlatform()) {
      // Open OAuth in system browser
      await Browser.open({
        url: `${API_URL}/api/auth/login/${provider}?redirect=myapp://auth/callback`,
        windowName: '_blank',
      });
    } else {
      // Web: standard redirect
      window.location.href = `/api/auth/login/${provider}`;
    }
  };

  // Listen for deep link callback
  useEffect(() => {
    if (Capacitor.isNativePlatform()) {
      App.addListener('appUrlOpen', async ({ url }) => {
        if (url.includes('myapp://auth/callback')) {
          await Browser.close();
          // Extract token from URL and store
          const token = extractTokenFromUrl(url);
          await SecureStorage.set({ key: 'authToken', value: token });
        }
      });
    }
  }, []);

  return { login };
}
```

### 12.5 Deep Link Configuration

**Android (android/app/src/main/AndroidManifest.xml):**
```xml
<intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="myapp" android:host="auth" />
</intent-filter>
```

**iOS (ios/App/App/Info.plist):**
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>myapp</string>
        </array>
    </dict>
</array>
```

### 12.6 Build and Deploy

```bash
# Build the React app
npm run build

# Sync with native projects
npx cap sync

# Open in native IDEs
npx cap open android  # Opens Android Studio
npx cap open ios      # Opens Xcode

# Live reload during development
npx cap run android --livereload --external
npx cap run ios --livereload --external
```

### 12.7 Mobile-Specific Considerations

| Consideration | Solution |
|---------------|----------|
| **Secure Token Storage** | Use `@capacitor/preferences` or platform keychain |
| **Network Connectivity** | Handle offline gracefully |
| **Push Notifications** | Use `@capacitor/push-notifications` |
| **Biometric Auth** | Use `capacitor-native-biometric` |
| **SSL Pinning** | Configure in native code |

---

## 13. API Design

### 13.1 Endpoints

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

### 13.2 Validation

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

### 13.3 API Documentation

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

## 14. Testing Strategy

### 14.1 Test Categories

| Category | Scope | Framework | Database |
|----------|-------|-----------|----------|
| **Unit Tests** | Single class | xUnit + Moq | None |
| **Integration Tests** | Workflow execution | xUnit | PostgreSQL + Redis (containerized) |
| **Emulator Tests** | Workflow logic | In-memory storage | In-memory |

### 14.2 Unit Test Examples

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

### 14.3 Integration Tests (Future)

For comprehensive testing with real PostgreSQL and Redis:

```csharp
public class IntegrationTests : IAsyncLifetime
{
    private IStorage _storage;
    private WorkflowExecutor _executor;

    public async Task InitializeAsync()
    {
        // Use testcontainers for PostgreSQL and Redis
        var redis = new RedisContainer().StartAsync();
        var postgres = new PostgreSqlContainer().StartAsync();
        
        await Task.WhenAll(redis, postgres);
        
        _storage = new RedisCheckpointStorage(
            ConnectionMultiplexer.Connect(redis.Result.GetConnectionString()));
        
        _executor = new WorkflowExecutor(_storage, new ActivityRegistry());
    }

    [Fact]
    public async Task FullWorkflow_ShouldComplete()
    {
        var input = new DocumentInput { /* ... */ };
        var workflowId = Guid.NewGuid().ToString();
        
        var updates = new List<WorkflowUpdate>();
        await foreach (var update in _executor.RunAsync(
            DocumentIngestionWorkflow.Create(_chatClient), 
            input, 
            workflowId))
        {
            updates.Add(update);
        }
        
        Assert.Contains(updates, u => u.Status == WorkflowStatus.Completed);
    }
}
```

---

## 15. Configuration Management

### 15.1 Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "Workflow": {
    "CheckpointTTLDays": 7
  },
  "OpenTelemetry": {
    "SamplingRatio": 1.0
  }
}

// appsettings.Production.json
{
  "Workflow": {
    "CheckpointTTLDays": 30
  },
  "OpenTelemetry": {
    "SamplingRatio": 0.1
  }
}
```

### 15.2 Connection Strings

**Development (via Aspire):**
```json
{
  "ConnectionStrings": {
    "appdb": "Host={postgres.bindings.tcp.host};Port={postgres.bindings.tcp.port};Database=appdb;Username=postgres;Password=...",
    "redis": "{redis.bindings.tcp.host}:{redis.bindings.tcp.port}"
  }
}
```

**Production:**
```json
{
  "ConnectionStrings": {
    "appdb": "Host=postgres-prod.internal;Port=5432;Database=appdb;Username=app_user;Password=...;SSL Mode=Require",
    "redis": "redis-prod.internal:6379,password=...,ssl=true"
  }
}
```

### 15.3 Authentication Configuration

```json
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

**Security Best Practices:**
- Store secrets in HashiCorp Vault, AWS Secrets Manager, or user secrets for development
- Use HTTPS for all authentication flows
- Implement CSRF protection for state tokens
- Set appropriate cookie security flags (HttpOnly, Secure, SameSite)

---

## 16. Deployment Considerations

### 16.1 Development (Aspire)

```bash
# Start all services
dotnet run --project MyApp.AppHost

# Dashboard available at https://localhost:15888
```

### 16.2 Docker Compose

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: appdb
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

  webapi:
    image: myapp-webapi:latest
    environment:
      ConnectionStrings__appdb: Host=postgres;Database=appdb;...
      ConnectionStrings__redis: redis:6379
      Authentication__Google__ClientId: ${GOOGLE_CLIENT_ID}
      Authentication__Google__ClientSecret: ${GOOGLE_CLIENT_SECRET}
    ports:
      - "8080:8080"
    depends_on:
      - postgres
      - redis

  worker:
    image: myapp-worker:latest
    environment:
      ConnectionStrings__appdb: Host=postgres;Database=appdb;...
      ConnectionStrings__redis: redis:6379
    depends_on:
      - postgres
      - redis

  frontend:
    image: myapp-frontend:latest
    ports:
      - "3000:80"
    depends_on:
      - webapi

volumes:
  postgres-data:
  redis-data:
```

### 16.3 Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-worker
spec:
  replicas: 1  # Single worker in Phase 1
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
        - name: ConnectionStrings__appdb
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: postgres-connection-string
        - name: ConnectionStrings__redis
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: redis-connection-string
        resources:
          requests:
            cpu: 500m
            memory: 256Mi
          limits:
            cpu: 1000m
            memory: 512Mi
```

---

## 17. Future Enhancements

### 17.1 Phase 2: Production Hardening

| Item | Description | Priority |
|------|-------------|----------|
| **Distributed Workers** | Redis Streams for horizontal scaling | High |
| **Retry Policies** | Configure activity retry with exponential backoff | High |
| **Dead Letter Queue** | Handle poison messages | High |
| **Workflow Versioning** | Support side-by-side workflow versions | Medium |

### 17.2 Phase 3: Advanced Features

| Item | Description | Priority |
|------|-------------|----------|
| **External Events** | Human-in-the-loop approval workflows | Medium |
| **Timers** | Scheduled activities and timeouts | Medium |
| **Sub-Workflows** | Break large workflows into child workflows | Low |
| **Saga Pattern** | Compensating transactions for failures | Low |

### 17.3 Phase 4: Operations

| Item | Description | Priority |
|------|-------------|----------|
| **Grafana Dashboards** | Pre-built observability dashboards | High |
| **Alerting** | Prometheus alerts for workflow failures | High |
| **Admin API** | Workflow management endpoints | Medium |
| **Purging** | Automatic cleanup of completed workflow checkpoints | Medium |

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

# EF Core migrations
dotnet ef migrations add MigrationName --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi
dotnet ef database update --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi

# Lint OpenAPI document
spectral lint src/MyApp.Server.WebApi/MyApp.Server.WebApi.json

# Frontend development
cd src/ui
npm install
npm run dev

# Build mobile apps
npm run build
npx cap sync
npx cap open android
npx cap open ios
```

### B.2 Key File Locations

| Purpose | Path |
|---------|------|
| Workflows | `src/MyApp.Server.Application/Workflows/` |
| Activities | `src/MyApp.Server.Infrastructure/Activities/` |
| Redis Storage | `src/MyApp.Server.Infrastructure/Storage/RedisCheckpointStorage.cs` |
| DbContext | `src/MyApp.Server.Infrastructure/Data/ApplicationDbContext.cs` |
| Entity Configs | `src/MyApp.Server.Infrastructure/Data/Configurations/` |
| AG-UI Server | `src/MyApp.Server.WebApi/Program.cs` |
| Auth Endpoints | `src/MyApp.Server.WebApi/Endpoints/AuthEndpoints.cs` |
| API Endpoints | `src/MyApp.Server.WebApi/Endpoints/` |
| Validators | `src/MyApp.Server.Application/Validators/` |
| Worker Host | `src/MyApp.Server.Worker/Program.cs` |
| Aspire Host | `src/MyApp.AppHost/Program.cs` |
| React Frontend | `src/ui/` |
| Capacitor Config | `src/ui/capacitor.config.ts` |
| Mobile Platforms | `src/mobile/android/`, `src/mobile/ios/` |
| OpenAPI Document | `src/MyApp.Server.WebApi/MyApp.Server.WebApi.json` |
| Spectral Config | `src/MyApp.Server.WebApi/.spectral.yml` |

### B.3 Troubleshooting

| Issue | Solution |
|-------|----------|
| Workflow checkpoint missing | Check Redis connection and key TTL settings |
| Activity timeout | Increase activity timeout or optimize activity code |
| Worker not processing | Check Redis connection and worker logs |
| OAuth redirect fails | Verify redirect URIs in provider console match `https://localhost:xxxx/api/auth/callback` |
| User not authenticated | Check cookie settings, ensure HTTPS, verify auth middleware order |
| EF migrations fail | Ensure connection string is correct, database exists, and correct startup project |
| AG-UI connection fails | Check CORS settings and authentication token |
| CopilotKit not connecting | Verify `runtimeUrl` and authentication headers |
| Mobile OAuth fails | Check deep link configuration in AndroidManifest.xml / Info.plist |
| Capacitor sync fails | Run `npm run build` before `npx cap sync` |

---

*End of Document*
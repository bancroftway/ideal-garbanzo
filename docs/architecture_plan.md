# MyApp Architecture Plan

> **Document Version:** 2.2  
> **Created:** December 15, 2025  
> **Last Updated:** December 27, 2025  
> **Status:** Architecture Redesign

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Project Goals and Objectives](#2-project-goals-and-objectives)
3. [Technology Stack](#3-technology-stack)
4. [Solution Structure](#4-solution-structure)
5. [Architecture Overview](#5-architecture-overview)
6. [Microsoft Agent Framework Workflows](#6-microsoft-agent-framework-workflows)
7. [Agentic Workflow Architecture](#7-agentic-workflow-architecture)
8. [Sample Application: Home Inspection](#8-sample-application-home-inspection)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [Observability Infrastructure](#10-observability-infrastructure)
11. [AG-UI Protocol & CopilotKit Integration](#11-ag-ui-protocol--copilotkit-integration)
12. [Frontend Authentication & Authorization](#12-frontend-authentication--authorization)
13. [Capacitor Mobile Apps](#13-capacitor-mobile-apps)
14. [API Design](#14-api-design)
15. [Testing Strategy](#15-testing-strategy)
16. [Configuration Management](#16-configuration-management)
17. [Deployment Considerations](#17-deployment-considerations)
18. [Future Enhancements](#18-future-enhancements)
19. [Appendix C: API Verification Status](#appendix-c-api-verification-status)

---

## 1. Executive Summary

### 1.1 Purpose

This document describes the architecture and implementation plan for **MyApp**, an **Agentic-First** self-hosted AI-powered application built using the **Microsoft Agent Framework Workflows** with Redis checkpoint persistence. 

**Agentic-First Architecture** means:
- **Every UI interaction** flows through the AG-UI protocol - including form filling, data entry, and navigation
- **Every backend operation** is an Agentic workflow with human-in-the-loop collaboration
- **Voice and text input** are first-class citizens for user interaction
- **Shared state** between frontend and agents enables real-time collaborative experiences

The solution features a React frontend powered by **CopilotKit** for conversational AI interactions (with voice support), communicating with the backend via the **AG-UI protocol** (Server-Sent Events streaming).

### 1.2 Key Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Architecture Philosophy** | Agentic-First | All interactions through AI agents, human-agent collaboration |
| **Orchestration Framework** | Microsoft Agent Framework Workflows | AI-native workflows, superstep checkpointing, long-running agent support |
| **Application Database** | PostgreSQL | Open-source, robust, excellent JSON support, self-hosted |
| **Workflow State Persistence** | Redis | Fast checkpoint storage, pub/sub for real-time updates, agent thread persistence |
| **Client-Server Protocol** | AG-UI (SSE) | Real-time streaming, human-in-the-loop, shared state, backend/frontend tools |
| **Voice Input** | Web Speech API + CopilotKit | Native browser speech recognition, seamless voice-to-text |
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
- **Agentic-First** - All user interactions must flow through AG-UI protocol, no direct REST API calls from UI for data operations
- **Minimal APIs only** - Presentation layer uses minimal APIs (no MVC/controllers)

---

## 2. Project Goals and Objectives

### 2.1 Primary Goals

1. **Agentic-First Architecture** - Every user interaction flows through AI agents; forms, data entry, and navigation are all agent-mediated
2. **Human-Agent Collaboration** - Enable seamless collaboration between users and AI agents with real-time shared state
3. **Voice-Enabled Interaction** - Support voice input alongside text for natural conversational experiences
4. **AI-Native Workflow Orchestration** - Leverage Microsoft Agent Framework for building intelligent, long-running AI workflows
5. **Real-Time User Experience** - Stream AI responses to users via AG-UI protocol with CopilotKit components
6. **Cross-Platform Clients** - Single React codebase for web (Vite) and mobile (Capacitor) applications
7. **Self-Hosted Solution** - Create a solution that can run entirely on-premises or in any cloud
8. **Session Persistence** - Save and resume agent conversations and workflow state across sessions

### 2.2 Secondary Goals

1. **Comprehensive Observability** - Full tracing and metrics for workflow execution
2. **Clean Architecture** - Demonstrate proper layering and separation of concerns
3. **Robust Validation** - Strong input validation at API boundaries
4. **Checkpoint Recovery** - Support workflow resumption after failures via Redis persistence
5. **Specialized Agent Orchestration** - Coordinate multiple specialized agents for complex tasks

### 2.3 Non-Goals

- Integration with Azure Functions runtime
- Azure Storage or Service Bus backends
- Multi-tenancy support (deferred to future phases)
- Separate background worker process (workflows execute inline with AG-UI requests)

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
| **Grafana** | 11.x | Unified dashboards for logs and metrics |
| **Loki** | 3.x | Log aggregation with OTLP ingestion |
| **Prometheus** | 3.x | Metrics storage and querying |

### 3.2 NuGet Packages

#### Microsoft Agent Framework

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI.Workflows` | latest 1.0.0-preview.* | Workflow orchestration with superstep checkpointing |
| `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` | latest 1.0.0-preview.* | AG-UI protocol server (SSE streaming) |
| `Microsoft.Extensions.AI` | 10.1.1 | AI abstractions and chat client interfaces |
| `Microsoft.Extensions.AI.OpenAI` | 10.1.1-preview.1.25612.2 | OpenAI/Azure OpenAI integration |

#### Validation

| Package | Version | Purpose |
|---------|---------|---------|
| `FluentValidation` | 12.1.1 | Fluent validation rules |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | DI integration |
| `SharpGrip.FluentValidation.AutoValidation.Endpoints` | 1.5.0 | Auto-validation for minimal APIs |

#### Observability

| Package | Version | Purpose |
|---------|---------|---------|
| `OpenTelemetry.Extensions.Hosting` | 1.14.0 | Hosting integration |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.14.0 | OTLP exporter |
| `OpenTelemetry.Exporter.Prometheus.AspNetCore` | 1.14.0-beta.1 | Prometheus metrics endpoint |
| `OpenTelemetry.Instrumentation.AspNetCore` | 1.14.0 | ASP.NET Core instrumentation |
| `OpenTelemetry.Instrumentation.Http` | 1.14.0 | HTTP client instrumentation |
| `OpenTelemetry.Instrumentation.StackExchangeRedis` | 1.14.0-beta.1 | Redis instrumentation |

#### API Documentation

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.1 | Native OpenAPI 3.1 document generation |
| `Microsoft.Extensions.ApiDescription.Server` | 10.0.1 | Build-time OpenAPI document generation |
| `Scalar.AspNetCore` | 2.11.10 | Modern interactive API documentation UI |

#### Testing

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.3 | Test framework |
| `Moq` | 4.20.72 | Mocking framework |
| `FluentAssertions` | 8.8.0 | Fluent assertions |

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
| `Microsoft.Extensions.ServiceDiscovery` | 10.1.0 | Aspire client service discovery |

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
| `@capacitor/core` | 8.0.0 | Core Capacitor runtime |
| `@capacitor/cli` | 8.0.0 | CLI for building mobile apps |
| `@capacitor/android` | 8.0.0 | Android platform support |
| `@capacitor/ios` | 8.0.0 | iOS platform support |
| `@capacitor/browser` | 8.0.0 | In-app browser for OAuth flows |

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
│   │   ├── MyApp.Server.WebApi/          # API + AG-UI server + Workflow execution
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
| **MyApp.Server.Application** | Application | Workflow definitions, validators, use cases, Agent definitions |
| **MyApp.Server.Infrastructure** | Infrastructure | Activities, middleware, external integrations, DbContext, Redis storage |
| **MyApp.Server.WebApi** | Presentation | HTTP endpoints, AG-UI server, authentication, inline workflow execution |
| **MyApp.AppHost** | Orchestration | Aspire host, resource provisioning |
| **MyApp.ServiceDefaults** | Cross-cutting | OpenTelemetry, health checks, service config |
| **MyApp.Shared** | Shared | DTOs shared between server and clients |
| **MyApp.Tests** | Testing | Unit tests, integration tests |
| **ui/** | Frontend | React + CopilotKit web application (Agentic-First UI) |
| **mobile/** | Frontend | Capacitor iOS/Android native shells |

### 4.3 Dependency Flow

```
                    ┌─────────────────┐
                    │  MyApp.AppHost  │
                    │  (Orchestrates) │
                    └────────┬────────┘
                             │
         ┌───────────────────┴───────────────────┐
         │                                       │
         ▼                                       ▼
  ┌─────────────────┐                   ┌───────────────┐
  │     WebApi      │                   │ServiceDefaults│
  │  + AG-UI        │                   └───────────────┘
  │  + Workflows    │                           ▲
  └────────┬────────┘                           │
           │                                    │
           ▼                                    │
       ┌─────────────────────┐                  │
       │   Infrastructure    │──────────────────┘
       │  (Redis, Postgres)  │
       └──────────┬──────────┘
                  │
                  ▼
       ┌─────────────────────┐
       │    Application      │
       │ (Workflows, Agents) │
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

#### Presentation Layer (MyApp.Server.WebApi)

Contains:
- **Endpoints** - HTTP request handlers and AG-UI server
- **Models** - API-specific request/response models
- **Configuration** - Application startup and DI setup
- **Inline Workflows** - Agent orchestration and workflow execution

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
              ┌──────────────────┴──────────────────┐
              │                                     │
              ▼                                     ▼
       ┌─────────────┐                       ┌─────────────┐
       │ PostgreSQL  │                       │    Redis    │
       │   (appdb)   │                       │             │
       │             │                       │ - Checkpts  │
       │ - Users     │                       │ - Threads   │
       │ - Documents │                       │ - Cache     │
       │ - Identity  │                       └─────────────┘
       └─────────────┘
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
    Task<IDictionary<string, T?>> ReadAsync<T>(
        IReadOnlyList<string> keys,
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

**Custom implementation required** - No pre-built Redis storage exists. The implementation below follows best practices for batch operations with proper error handling:

```csharp
public class RedisCheckpointStorage : IStorage
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCheckpointStorage> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private const string KeyPrefix = "workflow:checkpoint:";
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(7);

    public RedisCheckpointStorage(
        IConnectionMultiplexer redis,
        ILogger<RedisCheckpointStorage> logger)
    {
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public async Task<IDictionary<string, T?>> ReadAsync<T>(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var result = new Dictionary<string, T?>(StringComparer.Ordinal);

        if (keys.Count == 0)
            return result;

        // Use batch for atomic read of multiple keys
        var batch = db.CreateBatch();
        var tasks = new Dictionary<string, Task<RedisValue>>(keys.Count);

        foreach (var key in keys)
        {
            var redisKey = $"{KeyPrefix}{key}";
            tasks[key] = batch.StringGetAsync(redisKey);
        }

        batch.Execute();

        // Await all tasks and collect results with error handling
        var exceptions = new List<Exception>();

        foreach (var (key, task) in tasks)
        {
            try
            {
                var value = await task.ConfigureAwait(false);
                if (!value.HasValue)
                {
                    result[key] = default;
                    continue;
                }

                result[key] = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read checkpoint key: {Key}", key);
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0 && exceptions.Count == keys.Count)
        {
            throw new AggregateException("All checkpoint reads failed", exceptions);
        }

        return result;
    }
    
    public async Task WriteAsync<TStoreItem>(
        IDictionary<string, TStoreItem> changes, 
        CancellationToken cancellationToken)
    {
        if (changes.Count == 0)
            return;

        var db = _redis.GetDatabase();
        var batch = db.CreateBatch();
        var tasks = new List<(string Key, Task<bool> Task)>();
        
        foreach (var (key, value) in changes)
        {
            var redisKey = $"{KeyPrefix}{key}";
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            
            var task = batch.StringSetAsync(redisKey, json, DefaultExpiry);
            tasks.Add((key, task));
        }
        
        batch.Execute();
        
        // Properly await and handle each result
        var failures = new List<string>();
        
        foreach (var (key, task) in tasks)
        {
            try
            {
                var success = await task;
                if (!success)
                {
                    failures.Add(key);
                    _logger.LogWarning("Failed to write checkpoint: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                failures.Add(key);
                _logger.LogError(ex, "Exception writing checkpoint: {Key}", key);
            }
        }
        
        if (failures.Count > 0)
        {
            _logger.LogError(
                "Failed to write {Count}/{Total} checkpoints: {Keys}", 
                failures.Count, changes.Count, string.Join(", ", failures));
            
            if (failures.Count == changes.Count)
            {
                throw new InvalidOperationException(
                    $"All checkpoint writes failed: {string.Join(", ", failures)}");
            }
        }
    }
    
    public async Task DeleteAsync(
        string[] keys, 
        CancellationToken cancellationToken)
    {
        if (keys.Length == 0)
            return;

        var db = _redis.GetDatabase();
        var redisKeys = keys.Select(k => (RedisKey)$"{KeyPrefix}{k}").ToArray();
        
        try
        {
            var deletedCount = await db.KeyDeleteAsync(redisKeys);
            _logger.LogDebug(
                "Deleted {Deleted}/{Total} checkpoint keys", 
                deletedCount, keys.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete checkpoint keys");
            throw;
        }
    }

    /// <summary>
    /// Transaction-based write for atomic operations with optimistic concurrency.
    /// </summary>
    public async Task WriteAtomicAsync<TStoreItem>(
        IDictionary<string, TStoreItem> changes,
        string? expectedVersion,
        CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var transaction = db.CreateTransaction();
        
        // Optimistic concurrency check via version key
        if (!string.IsNullOrEmpty(expectedVersion))
        {
            var versionKey = $"{KeyPrefix}__version__";
            transaction.AddCondition(Condition.StringEqual(versionKey, expectedVersion));
        }
        
        var tasks = new List<Task<bool>>();
        
        foreach (var (key, value) in changes)
        {
            var redisKey = $"{KeyPrefix}{key}";
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            tasks.Add(transaction.StringSetAsync(redisKey, json, DefaultExpiry));
        }
        
        // Update version for next write
        var newVersion = Guid.NewGuid().ToString("N");
        tasks.Add(transaction.StringSetAsync($"{KeyPrefix}__version__", newVersion));
        
        var committed = await transaction.ExecuteAsync();
        
        if (!committed)
        {
            throw new ConcurrencyException(
                "Checkpoint update failed due to version conflict");
        }
        
        // Verify all writes succeeded
        foreach (var task in tasks)
        {
            await task; // Will throw if any individual write failed
        }
    }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
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

### 6.5 Superstep Checkpointing

The Microsoft Agent Framework uses **Pregel-style superstep checkpointing** for workflow state persistence. Unlike event-sourcing frameworks that replay every await, superstep checkpointing saves state at defined boundaries.

#### What Gets Captured

At each checkpoint boundary, the following state is persisted:

| State Type | Description |
|------------|-------------|
| **Pending Messages** | Unprocessed messages in agent queues |
| **Shared State** | `WorkflowContext.SharedState` dictionary |
| **Step Progress** | Current superstep number and completion status |
| **Agent Conversations** | Active agent thread states |

#### Checkpoint Hooks

```csharp
// Executor hooks for checkpoint events
public class InspectionWorkflowExecutor : WorkflowExecutor
{
    protected override async Task OnCheckpointingAsync(
        string workflowId,
        string checkpointId,
        IDictionary<string, object> state,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating checkpoint {CheckpointId} for workflow {WorkflowId}",
            checkpointId, workflowId);
        
        // Custom pre-checkpoint logic (e.g., flush caches)
        await FlushPendingUpdatesAsync(workflowId, cancellationToken);
    }

    protected override async Task OnCheckpointRestoredAsync(
        string workflowId,
        string checkpointId,
        IDictionary<string, object> state,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Restored checkpoint {CheckpointId} for workflow {WorkflowId}",
            checkpointId, workflowId);
        
        // Custom post-restore logic (e.g., rebuild caches)
        await RebuildInMemoryStateAsync(state, cancellationToken);
    }
}
```

#### Checkpoint Boundaries

Define checkpoint boundaries in workflow definitions:

```csharp
var workflow = new WorkflowBuilder<InspectionInput, InspectionOutput>()
    .AddStep("initialize", InitializeInspection)
    .WithCheckpoint("after-init")  // Checkpoint boundary
    .AddStep("generate-checklists", GenerateChecklists)
    .WithCheckpoint("checklists-ready")  // Checkpoint boundary
    .AddStep("process-findings", ProcessFindings)
    .WithCheckpoint("findings-complete")  // Checkpoint boundary
    .AddStep("generate-report", GenerateReport)
    .Build();
```

### 6.6 Comparison: Agent Framework vs DTFx

| Feature | Agent Framework | DTFx |
|---------|-----------------|------|
| **Checkpointing** | Superstep-level | Event-level (every await) |
| **Recovery** | Explicit via `ResumeStreamAsync` | Automatic deterministic replay |
| **State Storage** | Custom `IStorage` (Redis) | SQL Server tables |
| **AI Integration** | Native (Microsoft.Extensions.AI) | Manual integration |
| **Streaming** | AG-UI protocol (SSE) | Polling-based |
| **Human-in-Loop** | Built-in support | External events |
| **Distributed** | Inline with AG-UI | Multiple workers polling |

---

## 7. Agentic Workflow Architecture

### 7.1 Inline Workflow Execution

In the Agentic-First architecture, **all workflows execute inline** with AG-UI requests. There is no separate background worker process. The WebApi hosts both the AG-UI server and the workflow executor.

```
┌─────────────────────────────────────────────────────────────┐
│                    WebApi Process                            │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  AG-UI Server                                       │   │
│  │  - Receives SSE connections                         │   │
│  │  - Manages agent conversations                      │   │
│  │  - Streams responses to clients                     │   │
│  └───────────────────────┬─────────────────────────────┘   │
│                          │                                  │
│  ┌───────────────────────▼─────────────────────────────┐   │
│  │  Agent Orchestrator                                 │   │
│  │  - Coordinates specialized agents                   │   │
│  │  - Manages shared state                             │   │
│  │  - Handles human-in-the-loop interactions           │   │
│  └───────────────────────┬─────────────────────────────┘   │
│                          │                                  │
│  ┌───────────────────────▼─────────────────────────────┐   │
│  │  WorkflowExecutor                                   │   │
│  │  - Runs agentic workflows                           │   │
│  │  - Manages checkpoints                              │   │
│  │  - Persists agent thread state                      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
          │                              │
          ▼                              ▼
   ┌─────────────┐                ┌─────────────┐
   │    Redis    │                │ PostgreSQL  │
   │ Checkpoints │                │   (appdb)   │
   │  + Threads  │                └─────────────┘
   └─────────────┘
```

### 7.2 Aspire Configuration

```csharp
// Program.cs in MyApp.AppHost
var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL for application data
var postgres = builder.AddPostgres("postgres")
    .AddDatabase("appdb");

// Redis for workflow checkpoints and agent thread persistence
var redis = builder.AddRedis("redis");

// Observability Stack (raw containers - no Aspire hosting packages yet)
var loki = builder.AddContainer("loki", "grafana/loki", "3.4.3")
    .WithBindMount("./config/loki.yaml", "/etc/loki/local-config.yaml", isReadOnly: true)
    .WithHttpEndpoint(port: 3100, targetPort: 3100, name: "http")
    .WithHttpEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc")
    .WithHttpEndpoint(port: 4318, targetPort: 4318, name: "otlp-http");

var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.2.1")
    .WithBindMount("./config/prometheus.yaml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
    .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http");

var grafana = builder.AddContainer("grafana", "grafana/grafana", "11.6.0")
    .WithBindMount("./config/grafana/datasources.yaml", "/etc/grafana/provisioning/datasources/datasources.yaml", isReadOnly: true)
    .WithHttpEndpoint(port: 3001, targetPort: 3000, name: "http")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
    .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin")
    .WithReference(loki)
    .WithReference(prometheus)
    .WaitFor(loki)
    .WaitFor(prometheus);

// WebApi with AG-UI server and inline workflow execution
var webapi = builder.AddProject<Projects.MyApp_Server_WebApi>("webapi")
    .WithReference(postgres)
    .WithReference(redis)
    .WithEnvironment("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", loki.GetEndpoint("otlp-http"))
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(loki);

// React + CopilotKit frontend (Agentic-First UI)
builder.AddViteApp("frontend", "../ui")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithReference(webapi)
    .WaitFor(webapi);

builder.Build().Run();
```

### 7.3 Session Persistence

Agent conversations and workflow state are persisted to Redis using the `IStorage` interface, enabling users to resume sessions:

```csharp
// Agent thread persistence using IStorage interface
public class AgentThreadStorageService
{
    private readonly IStorage _storage;
    private readonly ILogger<AgentThreadStorageService> _logger;
    
    // Key pattern: agent:thread:{userId}:{threadId}
    private const string ThreadKeyPattern = "agent:thread:{0}:{1}";
    private const string UserThreadListKeyPattern = "agent:threads:{0}";
    
    public AgentThreadStorageService(
        IStorage storage,
        ILogger<AgentThreadStorageService> logger)
    {
        _storage = storage;
        _logger = logger;
    }
    
    public async Task SaveThreadAsync(
        string userId, 
        string threadId, 
        AgentThread thread,
        CancellationToken cancellationToken)
    {
        var key = string.Format(ThreadKeyPattern, userId, threadId);
        var listKey = string.Format(UserThreadListKeyPattern, userId);
        
        // Save thread and update user's thread list
        var changes = new Dictionary<string, object?>
        {
            [key] = thread,
            [listKey] = await GetUpdatedThreadList(userId, threadId, thread, cancellationToken)
        };
        
        await _storage.WriteAsync(changes, cancellationToken);
        _logger.LogDebug("Saved thread {ThreadId} for user {UserId}", threadId, userId);
    }
    
    public async Task<AgentThread?> LoadThreadAsync(
        string userId, 
        string threadId,
        CancellationToken cancellationToken)
    {
        var key = string.Format(ThreadKeyPattern, userId, threadId);
        var result = await _storage.ReadAsync<AgentThread?>([key], cancellationToken);

        return result.TryGetValue(key, out var value) ? value : null;
    }
    
    public async Task<IEnumerable<AgentThreadSummary>> ListThreadsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var listKey = string.Format(UserThreadListKeyPattern, userId);
        var result = await _storage.ReadAsync<List<AgentThreadSummary>?>([listKey], cancellationToken);

        if (result.TryGetValue(listKey, out var value) && value is not null)
        {
            return value;
        }

        return [];
    }
    
    public async Task DeleteThreadAsync(
        string userId,
        string threadId,
        CancellationToken cancellationToken)
    {
        var key = string.Format(ThreadKeyPattern, userId, threadId);
        await _storage.DeleteAsync([key], cancellationToken);
        
        // Update thread list to remove this thread
        await RemoveFromThreadList(userId, threadId, cancellationToken);
    }
    
    private async Task<List<AgentThreadSummary>> GetUpdatedThreadList(
        string userId,
        string threadId,
        AgentThread thread,
        CancellationToken cancellationToken)
    {
        var existing = (await ListThreadsAsync(userId, cancellationToken)).ToList();
        var summary = new AgentThreadSummary
        {
            ThreadId = threadId,
            Title = thread.Title,
            LastUpdated = DateTime.UtcNow
        };
        
        var index = existing.FindIndex(t => t.ThreadId == threadId);
        if (index >= 0)
            existing[index] = summary;
        else
            existing.Insert(0, summary);
        
        return existing;
    }
}

public record AgentThreadSummary
{
    public required string ThreadId { get; init; }
    public string? Title { get; init; }
    public DateTime LastUpdated { get; init; }
}
```

### 7.4 Future Scaling (Phase 2)

For high-volume scenarios, horizontal scaling can be achieved with Redis Streams:

| Phase | Architecture | Use Case |
|-------|-------------|----------|
| **Phase 1** | Single WebApi with inline workflows | Development, moderate traffic |
| **Phase 2** | Multiple WebApi instances + Redis Streams | Production, high-volume |

---

## 8. Sample Application: Home Inspection

### 8.1 Application Overview

The **Home Inspection** application demonstrates the Agentic-First architecture with specialized agents collaborating with a human inspector. This mirrors the human-agent collaboration pattern shown in the AG-UI Dojo demos.

**User Story:**
> A home inspector initiates an inspection by describing the property (bedrooms, bathrooms, kitchen, shed, etc.). Specialized AI agents generate tailored checklists for each area. As the inspector works through the property, agents continuously process findings, add follow-up questions, and generate the final report. Inspections can be paused and resumed across sessions.

### 8.2 Multi-Agent Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Home Inspection Workflow                              │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    Orchestrator Agent                                │   │
│  │  - Coordinates specialized agents                                   │   │
│  │  - Manages inspection state                                         │   │
│  │  - Synthesizes final report                                         │   │
│  └───────────────────────────┬─────────────────────────────────────────┘   │
│                              │                                              │
│      ┌───────────┬───────────┼───────────┬───────────┬───────────┐        │
│      │           │           │           │           │           │        │
│      ▼           ▼           ▼           ▼           ▼           ▼        │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐      │
│  │  Roof  │ │ Found- │ │Bedroom │ │  Bath- │ │Kitchen │ │ Extern-│      │
│  │ Agent  │ │ ation  │ │ Agent  │ │  room  │ │ Agent  │ │   al   │      │
│  │        │ │ Agent  │ │        │ │ Agent  │ │        │ │ Agent  │      │
│  └────┬───┘ └────┬───┘ └────┬───┘ └────┬───┘ └────┬───┘ └────┬───┘      │
│       │          │          │          │          │          │          │
│       └──────────┴──────────┴──────────┴──────────┴──────────┘          │
│                              │                                           │
│                              ▼                                           │
│              ┌───────────────────────────────────┐                      │
│              │     Human Inspector (via UI)      │                      │
│              │  - Voice/text input               │                      │
│              │  - Checklist completion           │                      │
│              │  - Photo attachments              │                      │
│              │  - Follow-up responses            │                      │
│              └───────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Specialized Agents

| Agent | Responsibility | Checklist Items |
|-------|---------------|-----------------|
| **RoofAgent** | Roof and attic inspection | Shingles, gutters, flashing, ventilation, leaks |
| **FoundationAgent** | Foundation and structure | Cracks, moisture, drainage, settling, structural integrity |
| **BedroomAgent** | Bedroom inspection | Windows, outlets, closets, smoke detectors, flooring |
| **BathroomAgent** | Bathroom inspection | Plumbing, ventilation, fixtures, water pressure, mold |
| **KitchenAgent** | Kitchen inspection | Appliances, cabinets, counters, electrical, plumbing |
| **ExternalAgent** | Exterior and grounds | Siding, doors, walkways, landscaping, drainage |

### 8.4 Multi-Agent State Sharing

Multiple specialized agents coordinate through shared state mechanisms provided by the Agent Framework:

#### WorkflowContext Shared State

Agents share state during workflow execution via `WorkflowContext.SharedState`:

```csharp
// Orchestrator sets shared state for all agents
public class InspectionOrchestrator
{
    public async Task ExecuteStepAsync(WorkflowContext context)
    {
        // Set shared state accessible by all agents
        context.SharedState["property"] = propertyStructure;
        context.SharedState["findings"] = new List<Finding>();
        context.SharedState["currentArea"] = "roof";
        
        // Each agent can read and update shared state
        await _roofAgent.InspectAsync(context);
        
        // Shared state persists across checkpoints
        var findings = (List<Finding>)context.SharedState["findings"];
    }
}

// Specialized agent reads shared state
public class RoofAgent
{
    public async Task InspectAsync(WorkflowContext context)
    {
        var property = (PropertyStructure)context.SharedState["property"];
        var findings = (List<Finding>)context.SharedState["findings"];
        
        // Add findings to shared state
        findings.Add(new Finding { Area = "roof", Issue = "Missing shingles" });
    }
}
```

#### AG-UI State Injection

Frontend state is injected into agents via the `ag_ui_state` property in AG-UI requests:

```csharp
// State from frontend is available in agent context
public class HomeInspectionAgent : ChatClientAgent
{
    protected override async Task OnTurnAsync(ITurnContext turnContext)
    {
        // Access frontend state injected via AG-UI
        var agUiState = turnContext.Activity.Value as JObject;
        var currentChecklist = agUiState?["currentChecklist"]?.ToObject<Checklist>();
        var inspectorLocation = agUiState?["location"]?.ToObject<GeoLocation>();
        
        // Process with frontend context
        await ProcessWithContextAsync(currentChecklist, inspectorLocation);
    }
}
```

```tsx
// Frontend: inject state into AG-UI requests
function InspectionView() {
    const { currentChecklist, location } = useInspectionState();
    
    return (
        <CopilotKit
            runtimeUrl="/api/agent"
            agentState={{
                currentChecklist,
                location,
                timestamp: Date.now()
            }}
        >
            <InspectionUI />
        </CopilotKit>
    );
}
```

#### ChatHistory Sharing Between Agents

Agents can share conversation history for context continuity:

```csharp
// Pass conversation context between specialized agents
public async Task HandoffToAgentAsync(
    string targetAgentName,
    IList<ChatMessage> relevantHistory,
    WorkflowContext context)
{
    var targetAgent = _specializedAgents[targetAgentName];
    
    // Inject relevant history as context
    var contextMessages = relevantHistory
        .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
        .TakeLast(10)  // Last 10 messages for context
        .ToList();
    
    // Store in shared state for target agent
    context.SharedState[$"history:{targetAgentName}"] = contextMessages;
    
    await targetAgent.ProcessAsync(context);
}
```

### 8.5 Workflow Implementation

```csharp
public class HomeInspectionOrchestrator
{
    private readonly Dictionary<string, IChatClientAgent> _specializedAgents;
    private readonly AgentThreadStorageService _threadStorage;
    private readonly IStorage _checkpointStorage;

    public async IAsyncEnumerable<AgentUpdate> StartInspectionAsync(
        HomeInspectionInput input,
        string userId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Create or resume inspection thread
        var threadId = input.InspectionId ?? Guid.NewGuid().ToString();
        var thread = await _threadStorage.LoadThreadAsync(userId, threadId, cancellationToken) 
            ?? new AgentThread { Id = threadId };

        yield return new AgentUpdate { 
            Type = "thread_created", 
            ThreadId = threadId 
        };

        // Initialize property structure
        var propertyStructure = new PropertyStructure
        {
            Bedrooms = input.BedroomCount,
            Bathrooms = input.BathroomCount,
            HasKitchen = true,
            HasBasement = input.HasBasement,
            HasGarage = input.HasGarage,
            ExternalStructures = input.ExternalStructures
        };

        // Generate checklists from specialized agents
        var checklists = new Dictionary<string, InspectionChecklist>();
        
        foreach (var (area, agent) in GetRequiredAgents(propertyStructure))
        {
            yield return new AgentUpdate { 
                Type = "generating_checklist", 
                Area = area 
            };

            var checklist = await agent.GenerateChecklistAsync(propertyStructure);
            checklists[area] = checklist;
            
            yield return new AgentUpdate { 
                Type = "checklist_ready", 
                Area = area,
                Checklist = checklist 
            };
        }

        // Save checkpoint using IStorage
        var checkpointKey = $"inspection:{threadId}:checkpoint:checklists-generated";
        await _checkpointStorage.WriteAsync(new Dictionary<string, object>
        {
            [checkpointKey] = new
            {
                PropertyStructure = propertyStructure,
                Checklists = checklists,
                Timestamp = DateTime.UtcNow
            }
        }, cancellationToken);

        // Stream shared state to frontend for UI rendering
        yield return new AgentUpdate {
            Type = "state_sync",
            State = new InspectionState
            {
                ThreadId = threadId,
                PropertyStructure = propertyStructure,
                Checklists = checklists,
                CurrentArea = checklists.Keys.First()
            }
        };
    }
}
```

### 8.6 Human-in-the-Loop Collaboration

The inspection workflow continuously processes inspector input and generates follow-ups:

```csharp
public async IAsyncEnumerable<AgentUpdate> ProcessInspectorInputAsync(
    string threadId,
    string userId,
    InspectorInput input,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var thread = await _threadStorage.LoadThreadAsync(userId, threadId, cancellationToken);
    var agent = _specializedAgents[input.CurrentArea];

    // Process the inspector's finding
    var analysis = await agent.AnalyzeFindingAsync(new FindingContext
    {
        ChecklistItem = input.ChecklistItem,
        InspectorNotes = input.Notes,
        Severity = input.Severity,
        Photos = input.PhotoUrls
    });

    yield return new AgentUpdate
    {
        Type = "finding_analyzed",
        Analysis = analysis
    };

    // Generate follow-up questions if needed
    if (analysis.RequiresFollowUp)
    {
        yield return new AgentUpdate
        {
            Type = "followup_required",
            Questions = analysis.FollowUpQuestions,
            Priority = analysis.Priority
        };
    }

    // Update checklist state
    yield return new AgentUpdate
    {
        Type = "checklist_updated",
        Item = input.ChecklistItem,
        Status = "completed",
        Findings = analysis.Summary
    };

    // Save progress checkpoint using IStorage (enables resume)
    var checkpointKey = $"inspection:{threadId}:checkpoint:item-{input.ChecklistItem}";
    var checkpointIndexKey = $"inspection:{threadId}:checkpoint-index";

    var indexResult = await _checkpointStorage.ReadAsync<List<string>?>(
        [checkpointIndexKey], cancellationToken);
    var index = indexResult.TryGetValue(checkpointIndexKey, out var existing) && existing is not null
        ? existing
        : new List<string>();

    if (!index.Contains(checkpointKey, StringComparer.Ordinal))
    {
        index.Add(checkpointKey);
    }

    await _checkpointStorage.WriteAsync(new Dictionary<string, object?>
    {
        [checkpointKey] = new
        {
            Input = input,
            Analysis = analysis,
            Timestamp = DateTime.UtcNow
        },
        [checkpointIndexKey] = index
    }, cancellationToken);
}
```

### 8.7 Resume Inspection Workflow

Inspectors can pause and resume inspections across sessions:

```csharp
public async IAsyncEnumerable<AgentUpdate> ResumeInspectionAsync(
    string threadId,
    string userId,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    // Load existing thread and state
    var thread = await _threadStorage.LoadThreadAsync(userId, threadId, cancellationToken);
    if (thread == null)
    {
        yield return new AgentUpdate { Type = "error", Message = "Inspection not found" };
        yield break;
    }

    // Load latest checkpoint using explicit index (no wildcard keys)
    var indexKey = $"inspection:{threadId}:checkpoint-index";
    var indexResult = await _checkpointStorage.ReadAsync<List<string>?>([indexKey], cancellationToken);
    var checkpointKeys = indexResult.TryGetValue(indexKey, out var keyList) && keyList is not null
        ? keyList
        : new List<string>();

    CheckpointData? latestCheckpoint = null;

    if (checkpointKeys.Count > 0)
    {
        var checkpoints = await _checkpointStorage.ReadAsync<CheckpointData?>(checkpointKeys, cancellationToken);
        latestCheckpoint = checkpoints.Values
            .Where(c => c is not null)
            .OrderByDescending(c => c!.Timestamp)
            .FirstOrDefault();
    }
    
    yield return new AgentUpdate
    {
        Type = "inspection_resumed",
        State = latestCheckpoint?.State,
        LastActivity = latestCheckpoint?.Timestamp
    };

    // Restore UI state via shared state sync
    yield return new AgentUpdate
    {
        Type = "state_sync",
        State = latestCheckpoint?.State
    };

    yield return new AgentUpdate
    {
        Type = "ready",
        Message = "Inspection resumed. Continue where you left off."
    };
}
```

### 8.8 Final Report Generation

```csharp
public async IAsyncEnumerable<AgentUpdate> GenerateReportAsync(
    string threadId,
    string userId,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var thread = await _threadStorage.LoadThreadAsync(userId, threadId, cancellationToken);
    
    // Load all checkpoints using explicit index (no wildcard keys)
    var indexKey = $"inspection:{threadId}:checkpoint-index";
    var indexResult = await _checkpointStorage.ReadAsync<List<string>?>([indexKey], cancellationToken);
    var checkpointKeys = indexResult.TryGetValue(indexKey, out var keyList) && keyList is not null
        ? keyList
        : new List<string>();

    var allCheckpoints = checkpointKeys.Count == 0
        ? new Dictionary<string, CheckpointData?>()
        : await _checkpointStorage.ReadAsync<CheckpointData?>(checkpointKeys, cancellationToken);

    yield return new AgentUpdate { Type = "generating_report" };

    // Orchestrator synthesizes findings from all agents
    var allFindings = allCheckpoints.Values
        .Where(c => c is not null)
        .SelectMany(c => c!.Findings ?? []);
        
    var report = await _orchestratorAgent.SynthesizeReportAsync(new ReportContext
    {
        AllFindings = allFindings,
        PropertyStructure = thread.PropertyStructure,
        InspectorId = userId
    });

    yield return new AgentUpdate
    {
        Type = "report_ready",
        Report = report,
        DownloadUrl = await SaveReportAsync(report)
    };
}
```

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
- `Microsoft.Extensions.ServiceDiscovery` (10.1.0) - Service discovery client

**Migration Commands:**
```bash
# Add migration
dotnet ef migrations add InitialCreate --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi

# Update database
dotnet ef database update --project MyApp.Server.Infrastructure --startup-project MyApp.Server.WebApi
```

### 9.10 Graceful Shutdown Handling

AG-UI uses Server-Sent Events (SSE) for streaming, which requires proper connection draining during shutdown to avoid abrupt client disconnections.

#### SSE Connection Manager

```csharp
public interface ISseConnectionManager
{
    void RegisterConnection(string connectionId, HttpResponse response);
    void RemoveConnection(string connectionId);
    Task DrainConnectionsAsync(TimeSpan timeout, CancellationToken cancellationToken);
    int ActiveConnectionCount { get; }
}

public class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<string, HttpResponse> _connections = new();
    private readonly ILogger<SseConnectionManager> _logger;

    public SseConnectionManager(ILogger<SseConnectionManager> logger)
    {
        _logger = logger;
    }

    public void RegisterConnection(string connectionId, HttpResponse response)
    {
        _connections.TryAdd(connectionId, response);
        _logger.LogDebug("SSE connection registered: {ConnectionId}. Active: {Count}", 
            connectionId, _connections.Count);
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        _logger.LogDebug("SSE connection removed: {ConnectionId}. Active: {Count}", 
            connectionId, _connections.Count);
    }

    public int ActiveConnectionCount => _connections.Count;

    public async Task DrainConnectionsAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (_connections.IsEmpty)
            return;

        _logger.LogInformation("Draining {Count} SSE connections with {Timeout}s timeout", 
            _connections.Count, timeout.TotalSeconds);

        // Send shutdown notification to all clients
        var shutdownTasks = _connections.Values.Select(async response =>
        {
            try
            {
                await response.WriteAsync(
                    "event: shutdown\ndata: Server is shutting down\n\n", 
                    cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send shutdown notification");
            }
        });

        await Task.WhenAll(shutdownTasks);

        // Wait for connections to drain or timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        while (!_connections.IsEmpty && !cts.Token.IsCancellationRequested)
        {
            await Task.Delay(100, cts.Token);
        }

        if (!_connections.IsEmpty)
        {
            _logger.LogWarning("Shutdown completed with {Count} active SSE connections", 
                _connections.Count);
        }
    }
}
```

#### Shutdown Service

```csharp
public class SseShutdownService : IHostedService
{
    private readonly ISseConnectionManager _connectionManager;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<SseShutdownService> _logger;
    private readonly TimeSpan _drainTimeout;

    public SseShutdownService(
        ISseConnectionManager connectionManager,
        IHostApplicationLifetime lifetime,
        ILogger<SseShutdownService> logger,
        IConfiguration configuration)
    {
        _connectionManager = connectionManager;
        _lifetime = lifetime;
        _logger = logger;
        _drainTimeout = TimeSpan.FromSeconds(
            configuration.GetValue("Shutdown:DrainTimeoutSeconds", 30));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStopping.Register(OnApplicationStopping);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnApplicationStopping()
    {
        _logger.LogInformation("Application stopping. Draining SSE connections...");
        
        // Drain connections synchronously during shutdown
        _connectionManager.DrainConnectionsAsync(
            _drainTimeout, 
            CancellationToken.None).GetAwaiter().GetResult();
    }
}
```

#### Registration

```csharp
// Program.cs
builder.Services.AddSingleton<ISseConnectionManager, SseConnectionManager>();
builder.Services.AddHostedService<SseShutdownService>();
```

#### Configuration

```json
{
  "Shutdown": {
    "DrainTimeoutSeconds": 30
  }
}
```

---

## 10. Observability Infrastructure

This section describes the unified observability stack for logs, metrics, and dashboards using Grafana, Loki, and Prometheus.

### 10.1 Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         Observability Stack                              │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                     Grafana (Port 3001)                         │   │
│  │  - Unified dashboards                                           │   │
│  │  - Loki data source (logs)                                      │   │
│  │  - Prometheus data source (metrics)                             │   │
│  └────────────────────────┬────────────────────────────────────────┘   │
│                           │                                             │
│           ┌───────────────┴───────────────┐                             │
│           │                               │                             │
│           ▼                               ▼                             │
│  ┌─────────────────────┐        ┌─────────────────────┐                │
│  │   Loki (Port 3100)  │        │ Prometheus (Port 9090)│               │
│  │                     │        │                       │               │
│  │ - OTLP ingestion    │        │ - Scrape /metrics     │               │
│  │ - Log aggregation   │        │ - Time-series DB      │               │
│  │ - 14-day retention  │        │ - 15-day retention    │               │
│  └──────────┬──────────┘        └───────────┬───────────┘               │
│             │                               │                           │
│             │ OTLP/HTTP                     │ HTTP Scrape               │
│             │                               │                           │
│  ┌──────────┴───────────────────────────────┴──────────┐               │
│  │              .NET Applications                       │               │
│  │                                                      │               │
│  │  WebApi                                              │               │
│  │  - /metrics                                          │               │
│  │  - OTLP logs                                         │               │
│  └──────────────────────────────────────────────────────┘               │
└─────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Loki Configuration

Loki 3.x includes native OTLP ingestion support, eliminating the need for Promtail for .NET applications.

**config/loki.yaml:**
```yaml
auth_enabled: false

server:
  http_listen_port: 3100
  grpc_listen_port: 9096

common:
  instance_addr: 127.0.0.1
  path_prefix: /tmp/loki
  storage:
    filesystem:
      chunks_directory: /tmp/loki/chunks
      rules_directory: /tmp/loki/rules
  replication_factor: 1
  ring:
    kvstore:
      store: inmemory

# OTLP ingestion configuration
limits_config:
    allow_structured_metadata: true
    retention_period: 336h  # 14 days
    max_query_lookback: 336h
    otlp_config:
        resource_attributes:
            attributes_config:
                - action: index_label
                    attributes:
                        - service.name
                        - service.namespace
                        - deployment.environment
        scope_attributes:
            - action: drop
                attributes:
                    - telemetry.sdk.*
        log_attributes:
            - action: structured_metadata
                attributes:
                    - http.request.method
                    - http.response.status_code
                    - url.path
                    - user_agent.original

# Retention configuration (14 days default)
compactor:
    retention_enabled: true
    retention_delete_delay: 2h
    delete_request_store: filesystem

schema_config:
  configs:
    - from: 2024-01-01
      store: tsdb
      object_store: filesystem
      schema: v13
      index:
        prefix: index_
        period: 24h

query_range:
  results_cache:
    cache:
      embedded_cache:
        enabled: true
        max_size_mb: 100
```

### 10.3 Prometheus Configuration

Prometheus scrapes `/metrics` endpoints from .NET applications.

**config/prometheus.yaml:**
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'webapi'
    static_configs:
      - targets: ['webapi:8080']
    metrics_path: /metrics
    scheme: http

  # Prometheus self-monitoring
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

# Retention configuration (15 days default via command line)
# Start Prometheus with: --storage.tsdb.retention.time=15d
```

### 10.4 Grafana Configuration

Grafana is pre-configured with Loki and Prometheus as data sources.

**config/grafana/datasources.yaml:**
```yaml
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: false
    jsonData:
      httpMethod: POST
      manageAlerts: true
      prometheusType: Prometheus
      prometheusVersion: "3.2.1"

  - name: Loki
    type: loki
    access: proxy
    url: http://loki:3100
    editable: false
    jsonData:
      maxLines: 1000
      derivedFields:
        - datasourceUid: Prometheus
          matcherRegex: "trace_id=(\\w+)"
          name: TraceID
          url: "$${__value.raw}"
```

### 10.5 .NET Application Configuration

#### Prometheus Metrics Endpoint

Enable the Prometheus exporter in .NET applications:

```csharp
// ServiceDefaults/Extensions.cs
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("MyApp.Workflows")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();  // Exposes /metrics endpoint
    });

// Program.cs - Map the metrics endpoint
app.MapPrometheusScrapingEndpoint();  // Exposes GET /metrics
```

#### OTLP Logs to Loki

Configure OTLP log exporter to send directly to Loki:

```csharp
// ServiceDefaults/Extensions.cs
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    
    // OTLP exporter sends to Loki's OTLP endpoint
    // Configured via OTEL_EXPORTER_OTLP_LOGS_ENDPOINT environment variable
    logging.AddOtlpExporter();
});
```

The `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` environment variable is set in the Aspire AppHost configuration to point to Loki's OTLP HTTP endpoint.

### 10.6 OTLP Log Fields

.NET applications using OpenTelemetry emit structured logs with the following fields automatically:

| Field | Source | Description |
|-------|--------|-------------|
| `service.name` | Resource | Application name from Aspire |
| `service.namespace` | Resource | Namespace (if configured) |
| `deployment.environment` | Resource | Environment name |
| `http.request.method` | Span | HTTP method (GET, POST, etc.) |
| `http.response.status_code` | Span | Response status code |
| `url.path` | Span | Request URL path |
| `user_agent.original` | Span | Client user agent |
| `trace_id` | Context | Distributed trace ID |
| `span_id` | Context | Current span ID |
| `exception.type` | Event | Exception type name |
| `exception.message` | Event | Exception message |
| `exception.stacktrace` | Event | Stack trace |

### 10.7 Retention Configuration

| Component | Default Retention | Configuration |
|-----------|-------------------|---------------|
| **Loki** | 14 days (336h) | `limits_config.retention_period` in loki.yaml |
| **Prometheus** | 15 days | `--storage.tsdb.retention.time=15d` command line arg |
| **Grafana** | N/A (dashboards) | Dashboards are configuration, not time-series data |

### 10.8 Accessing the Observability Stack

| Service | URL | Purpose |
|---------|-----|---------|
| **Grafana** | http://localhost:3001 | Dashboards and visualization |
| **Loki** | http://localhost:3100 | Log queries (via Grafana) |
| **Prometheus** | http://localhost:9090 | Metric queries and targets |

### 10.9 Service Level Indicators (SLIs) and Objectives (SLOs)

#### SLI Definitions

| SLI | Definition | Measurement |
|-----|------------|-------------|
| **Availability** | Percentage of successful requests (non-5xx responses) | `sum(rate(http_server_request_duration_seconds_count{http_response_status_code!~"5.."}[5m])) / sum(rate(http_server_request_duration_seconds_count[5m]))` |
| **Latency (p95)** | 95th percentile response time for AG-UI requests | `histogram_quantile(0.95, sum(rate(http_server_request_duration_seconds_bucket{http_route="/api/agent"}[5m])) by (le))` |
| **Workflow Success** | Percentage of workflows completing successfully | `sum(workflow_completions{status="completed"}) / sum(workflow_completions)` |
| **Error Rate** | Rate of errors per second | `rate(workflow_errors_total[5m])` |

#### SLO Targets

| Service | SLI | Target | Window |
|---------|-----|--------|--------|
| AG-UI Endpoint | Availability | 99.5% | 30 days |
| AG-UI Endpoint | Latency (p95) | < 30s | 30 days |
| Workflows | Success Rate | 99% | 30 days |
| Authentication | Availability | 99.9% | 30 days |

#### Prometheus Alerting Rules

```yaml
# config/prometheus-alerts.yaml
groups:
  - name: myapp-slos
    rules:
      - alert: HighErrorRate
        expr: |
                    sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
                    / sum(rate(http_server_request_duration_seconds_count[5m])) > 0.005
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value | humanizePercentage }} (threshold: 0.5%)"
          
      - alert: AGUILatencyHigh
        expr: |
                    histogram_quantile(0.95,
                        sum(rate(http_server_request_duration_seconds_bucket{http_route="/api/agent"}[5m])) by (le)) > 30
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "AG-UI p95 latency above 30 seconds"
          description: "P95 latency is {{ $value | humanizeDuration }}"
          
      - alert: WorkflowFailureRate
        expr: |
          sum(rate(workflow_errors_total[5m])) 
          / sum(rate(workflow_completions[5m])) > 0.01
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Workflow failure rate above 1%"
          description: "Failure rate is {{ $value | humanizePercentage }}"
          
      - alert: RedisConnectionFailure
        expr: redis_connected_clients < 1
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Redis connection lost"
          description: "No connected clients to Redis"

      - alert: PostgresConnectionFailure
        expr: pg_up == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "PostgreSQL connection lost"
          description: "Cannot connect to PostgreSQL"
```

#### Prometheus Configuration Update

```yaml
# config/prometheus.yaml - add alerting configuration
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - prometheus-alerts.yaml

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          # - alertmanager:9093  # Add when Alertmanager is configured

scrape_configs:
  - job_name: 'webapi'
    static_configs:
      - targets: ['webapi:8080']
    metrics_path: /metrics
    scheme: http
```

### 10.10 Phase 2: Promtail for Container Logs

For non-.NET container logs (PostgreSQL, Redis, nginx), Promtail will be added in Phase 2:

```yaml
# Future: config/promtail.yaml
server:
  http_listen_port: 9080
  grpc_listen_port: 0

positions:
  filename: /tmp/positions.yaml

clients:
  - url: http://loki:3100/loki/api/v1/push

scrape_configs:
  - job_name: containers
    docker_sd_configs:
      - host: unix:///var/run/docker.sock
        refresh_interval: 5s
    relabel_configs:
      - source_labels: ['__meta_docker_container_name']
        regex: '/(.*)'
        target_label: 'container'
```

**Note:** Promtail is not needed for .NET applications in Phase 1 because they send logs directly to Loki via OTLP. Promtail will be added when container log collection for infrastructure services is required.

---

## 11. AG-UI Protocol & CopilotKit Integration

### 11.1 What is AG-UI?

The **AG-UI (Agent User Interaction) Protocol** is a standardized communication protocol between AI agents and user interfaces. It uses Server-Sent Events (SSE) for real-time streaming of AI responses.

### 11.2 AG-UI Server Setup

**WebApi Configuration (minimal APIs only, latest `Microsoft.Agents.AI.*` preview):**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.AddAuthorization();
builder.Services.AddAGUI();

// Configure the AI chat client and agent
var chatClient = builder.Services
    .AddOpenAIClient(builder.Configuration["OpenAI:ApiKey"])
    .AsIChatClient();

var tools = new[]
{
    AIFunctionFactory.Create(documentService.IngestDocumentAsync),
    AIFunctionFactory.Create(documentService.GetDocumentStatusAsync),
};

var agent = chatClient.CreateAIAgent(
    name: "DocumentAssistant",
    instructions: "You are a helpful assistant for document processing...",
    tools: tools);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Minimal API mapping for AG-UI (no MVC/controllers)
app.MapAGUI("/api/agent", agent)
    .RequireAuthorization();

app.Run();
```

### 11.3 AG-UI Protocol Features

| Feature | Description |
|---------|-------------|
| **Agentic Chat** | Real-time AI conversation streaming |
| **Backend Tools** | Server-side function execution |
| **Frontend Tools** | Client-side function execution |
| **Human-in-the-Loop** | User confirmation for sensitive actions |
| **Shared State** | Bidirectional state synchronization |
| **Generative UI** | Dynamic UI component rendering |

### 11.4 CopilotKit React Integration

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

### 11.5 Backend Tools (Server-Side Actions)

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

### 11.6 Human-in-the-Loop

Human-in-the-loop is implemented through **tool-based patterns** using AG-UI events. The backend defines sensitive tools, and the frontend handles confirmation UI via CopilotKit.

#### Backend Tool Definition

```csharp
// Backend tool that requires human confirmation
[Description("Delete a document permanently - requires user confirmation")]
public async Task<DeleteResult> DeleteDocumentAsync(
    [Description("Document ID to delete")] string documentId,
    CancellationToken cancellationToken)
{
    await _documentRepository.DeleteAsync(documentId);
    return new DeleteResult { Success = true, DocumentId = documentId };
}
```

#### Frontend Confirmation Handler

```tsx
// Frontend: Handle tool confirmation via useCopilotAction
import { useCopilotAction } from "@copilotkit/react-core";

function DocumentManager() {
    useCopilotAction({
        name: "deleteDocument",
        description: "Delete a document permanently",
        parameters: [
            { name: "documentId", type: "string", required: true }
        ],
        // renderAndWait enables human-in-the-loop confirmation
        renderAndWait: ({ args, status, handler }) => {
            if (status === "executing") {
                return (
                    <ConfirmationDialog
                        title="Delete Document"
                        message={`Are you sure you want to delete document ${args.documentId}?`}
                        onConfirm={() => handler.confirm()}
                        onCancel={() => handler.cancel()}
                    />
                );
            }
            return null;
        },
    });
    
    return <DocumentList />;
}
```

#### AG-UI Event Flow

The human-in-the-loop flow uses AG-UI protocol events:

1. **ToolCallStart** - Agent requests tool execution
2. **Frontend renders confirmation UI** - `renderAndWait` displays dialog
3. **User confirms or cancels** - `handler.confirm()` or `handler.cancel()`
4. **ToolCallEnd** - Tool execution completes (or is cancelled)
5. **ToolCallResult** - Result streamed back to agent

---

## 12. Frontend Authentication & Authorization

### 12.1 Authentication Architecture

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

### 12.2 WebApi Authentication Configuration

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
    options.Cookie.SameSite = SameSiteMode.Lax; // OAuth provider redirects require Lax/None
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

### 12.3 React Authentication Implementation

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

### 12.4 Authentication Endpoints

| Endpoint | Method | Auth Required | Purpose |
|----------|--------|---------------|---------|
| `/api/auth/login/{provider}` | GET | No | Initiate OAuth flow |
| `/api/auth/callback` | GET | No | OAuth callback handler |
| `/api/auth/logout` | POST | Yes | Sign out user |
| `/api/auth/user` | GET | No | Get current user info |
| `/api/agent` | POST | **Yes** | AG-UI streaming endpoint |
| `/api/documents/*` | ALL | **Yes** | Document management |

### 12.5 Security Best Practices

| Practice | Implementation |
|----------|----------------|
| **HTTPS Only** | All auth flows over TLS |
| **HttpOnly Cookies** | Prevent XSS token theft |
| **SameSite=Lax/None** | Lax for OAuth redirects; None requires Secure |
| **Secure Flag** | Cookie only sent over HTTPS |
| **Short Token Expiry** | 7 days with refresh |
| **CORS Configuration** | Restrict to known origins |
| **PKCE** | Required for mobile OAuth flows |
| **Redis TLS** | Encrypted connections to Redis |
| **Rate Limiting** | Protect against abuse |

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

#### PKCE for Mobile OAuth

Mobile OAuth flows require PKCE (Proof Key for Code Exchange) to prevent authorization code interception:

```csharp
// Generate PKCE code verifier and challenge
public static class PkceHelper
{
    public static (string CodeVerifier, string CodeChallenge) Generate()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        return (codeVerifier, codeChallenge);
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
```

#### Redis TLS Configuration

```csharp
// Secure Redis connection with TLS
var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { "redis-server:6380" },
    Ssl = true,
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
    Password = configuration["Redis:Password"],
    AbortOnConnectFail = false
});
```

#### CSRF Token Validation

```csharp
// CSRF state token validation using IDataProtectionProvider
public class CsrfStateValidator
{
    private readonly IDataProtector _protector;

    public CsrfStateValidator(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("OAuth.State");
    }

    public string GenerateState(string redirectUri)
    {
        var payload = new StatePayload
        {
            RedirectUri = redirectUri,
            Nonce = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        var json = JsonSerializer.Serialize(payload);
        return _protector.Protect(json);
    }

    public bool ValidateState(string state, out StatePayload? payload)
    {
        try
        {
            var json = _protector.Unprotect(state);
            payload = JsonSerializer.Deserialize<StatePayload>(json);
            
            // Check timestamp is within 10 minutes
            var timestamp = DateTimeOffset.FromUnixTimeSeconds(payload!.Timestamp);
            return DateTimeOffset.UtcNow - timestamp < TimeSpan.FromMinutes(10);
        }
        catch
        {
            payload = null;
            return false;
        }
    }
}
```

#### Rate Limiting for SSE Endpoints

```csharp
// Program.cs - Configure rate limiting with sliding window
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4
            }));

    // Special limiter for AG-UI endpoint (allows longer connections)
    options.AddPolicy("AgUi", context =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new ConcurrencyLimiterOptions
            {
                PermitLimit = 5,  // Max 5 concurrent SSE connections per user
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

// Apply rate limiting
app.UseRateLimiter();
app.MapAGUI("/api/agent", agent)
    .RequireAuthorization()
    .RequireRateLimiting("AgUi");
```

---

## 13. Capacitor Mobile Apps

### 13.1 Overview

**Capacitor** enables building native iOS and Android apps using the same React + CopilotKit codebase. The mobile apps share the web frontend code while gaining access to native device features.

### 13.2 Project Setup

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

### 13.3 Capacitor Configuration

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

### 13.4 Mobile Authentication

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

### 13.5 Deep Link Configuration

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

### 13.6 Build and Deploy

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

### 13.7 Mobile-Specific Considerations

| Consideration | Solution |
|---------------|----------|
| **Secure Token Storage** | Use `@capacitor/preferences` or platform keychain |
| **Network Connectivity** | Handle offline gracefully |
| **Push Notifications** | Use `@capacitor/push-notifications` |
| **Biometric Auth** | Use `capacitor-native-biometric` |
| **SSL Pinning** | Configure in native code |

---

## 14. API Design

### 14.1 Endpoints

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

### 14.2 Validation

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

### 14.3 API Documentation

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

## 15. Testing Strategy

### 15.1 Test Categories

| Category | Scope | Framework | Database |
|----------|-------|-----------|----------|
| **Unit Tests** | Single class | xUnit + Moq | None |
| **Integration Tests** | Workflow execution | xUnit | PostgreSQL + Redis (containerized) |
| **Emulator Tests** | Workflow logic | In-memory storage | In-memory |

### 15.2 Unit Test Examples

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

### 15.3 Integration Tests (Future)

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

## 16. Configuration Management

### 16.1 Environment-Specific Configuration

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

### 16.2 Connection Strings

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

### 16.3 Authentication Configuration

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

## 17. Deployment Considerations

### 17.1 Development (Aspire)

```bash
# Start all services
dotnet run --project MyApp.AppHost

# Dashboard available at https://localhost:15888
```

### 17.2 Docker Compose

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

  loki:
    image: grafana/loki:3.4.3
    ports:
      - "3100:3100"
      - "4317:4317"
      - "4318:4318"
    volumes:
      - ./config/loki.yaml:/etc/loki/local-config.yaml:ro
      - loki-data:/tmp/loki

  prometheus:
    image: prom/prometheus:v3.2.1
    ports:
      - "9090:9090"
    volumes:
      - ./config/prometheus.yaml:/etc/prometheus/prometheus.yml:ro
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.retention.time=15d'

  grafana:
    image: grafana/grafana:11.6.0
    ports:
      - "3001:3000"
    environment:
      GF_AUTH_ANONYMOUS_ENABLED: "true"
      GF_AUTH_ANONYMOUS_ORG_ROLE: Admin
    volumes:
      - ./config/grafana/datasources.yaml:/etc/grafana/provisioning/datasources/datasources.yaml:ro
      - grafana-data:/var/lib/grafana
    depends_on:
      - loki
      - prometheus

  webapi:
    image: myapp-webapi:latest
    environment:
      ConnectionStrings__appdb: Host=postgres;Database=appdb;...
      ConnectionStrings__redis: redis:6379
      Authentication__Google__ClientId: ${GOOGLE_CLIENT_ID}
      Authentication__Google__ClientSecret: ${GOOGLE_CLIENT_SECRET}
      OTEL_EXPORTER_OTLP_LOGS_ENDPOINT: http://loki:4318
    ports:
      - "8080:8080"
    depends_on:
      - postgres
      - redis
      - loki

  frontend:
    image: myapp-frontend:latest
    ports:
      - "3000:80"
    depends_on:
      - webapi

volumes:
  postgres-data:
  redis-data:
  loki-data:
  prometheus-data:
  grafana-data:
```

### 17.3 Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-webapi
spec:
  replicas: 1  # Single instance for Phase 1
  selector:
    matchLabels:
      app: myapp-webapi
  template:
    metadata:
      labels:
        app: myapp-webapi
    spec:
      containers:
      - name: webapi
        image: myapp-webapi:latest
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
        - name: Authentication__Google__ClientId
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: google-client-id
        resources:
          requests:
            cpu: 500m
            memory: 512Mi
          limits:
            cpu: 2000m
            memory: 1Gi
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
```

---

## 18. Future Enhancements

### 18.1 Phase 2: Production Hardening

| Item | Description | Priority |
|------|-------------|----------|
| **Horizontal Scaling** | Redis Streams for multiple WebApi instances | High |
| **Retry Policies** | Configure activity retry with exponential backoff | High |
| **Dead Letter Queue** | Handle poison messages | High |
| **Workflow Versioning** | Support side-by-side workflow versions | Medium |

### 18.2 Phase 3: Advanced Features

| Item | Description | Priority |
|------|-------------|----------|
| **External Events** | Human-in-the-loop approval workflows | Medium |
| **Timers** | Scheduled activities and timeouts | Medium |
| **Sub-Workflows** | Break large workflows into child workflows | Low |
| **Saga Pattern** | Compensating transactions for failures | Low |

### 18.3 Phase 4: Operations

| Item | Description | Priority |
|------|-------------|----------|
| **Promtail Integration** | Add Promtail for non-.NET container logs (PostgreSQL, Redis) | High |
| **Grafana Dashboards** | Pre-built dashboards for workflows, agents, and infrastructure | High |
| **Alerting Rules** | Prometheus alerting for workflow failures, error rates | High |
| **Admin API** | Workflow management endpoints | Medium |
| **Purging** | Automatic cleanup of completed workflow checkpoints | Medium |

---

## Appendix A: Architecture Diagram

For a complete visual representation of the solution architecture, including all projects and their dependency tree, see:

**[.github/copilot-instructions.md](.github/copilot-instructions.md)**

The diagram includes:
- All solution projects (WebApi, Client apps, Shared libraries)
- Project dependencies and references
- Data flow between components
- Database connections (appdb and Redis)
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
| Agent Thread Store | `src/MyApp.Server.Infrastructure/Storage/RedisAgentThreadStorage.cs` |
| DbContext | `src/MyApp.Server.Infrastructure/Data/ApplicationDbContext.cs` |
| Entity Configs | `src/MyApp.Server.Infrastructure/Data/Configurations/` |
| AG-UI Server | `src/MyApp.Server.WebApi/Program.cs` |
| Auth Endpoints | `src/MyApp.Server.WebApi/Endpoints/AuthEndpoints.cs` |
| API Endpoints | `src/MyApp.Server.WebApi/Endpoints/` |
| Validators | `src/MyApp.Server.Application/Validators/` |
| Aspire Host | `src/MyApp.AppHost/Program.cs` |
| React Frontend | `src/ui/` |
| Capacitor Config | `src/ui/capacitor.config.ts` |
| Mobile Platforms | `src/mobile/android/`, `src/mobile/ios/` |
| OpenAPI Document | `src/MyApp.Server.WebApi/MyApp.Server.WebApi.json` |
| Spectral Config | `src/MyApp.Server.WebApi/.spectral.yml` |
| Loki Config | `src/MyApp.AppHost/config/loki.yaml` |
| Prometheus Config | `src/MyApp.AppHost/config/prometheus.yaml` |
| Grafana Datasources | `src/MyApp.AppHost/config/grafana/datasources.yaml` |

### B.3 Troubleshooting

| Issue | Solution |
|-------|----------|
| Workflow checkpoint missing | Check Redis connection and key TTL settings |
| Activity timeout | Increase activity timeout or optimize activity code |
| OAuth redirect fails | Verify redirect URIs in provider console match `https://localhost:xxxx/api/auth/callback` |
| User not authenticated | Check cookie settings, ensure HTTPS, verify auth middleware order |
| EF migrations fail | Ensure connection string is correct, database exists, and correct startup project |
| AG-UI connection fails | Check CORS settings and authentication token |
| CopilotKit not connecting | Verify `runtimeUrl` and authentication headers |
| Mobile OAuth fails | Check deep link configuration in AndroidManifest.xml / Info.plist |
| Capacitor sync fails | Run `npm run build` before `npx cap sync` |
| Logs not appearing in Loki | Check OTEL_EXPORTER_OTLP_LOGS_ENDPOINT env var and Loki container status |
| Metrics not in Prometheus | Verify /metrics endpoint is accessible and Prometheus scrape config |
| Grafana shows no data | Check datasource configuration and verify Loki/Prometheus are running |

---

## Appendix C: API Verification Status

| Area | Status | Notes |
|------|--------|-------|
| Redis checkpoint storage (Section 6.3) | Verified against StackExchange.Redis 7.x and System.Text.Json | Typed `ReadAsync<T>` + explicit checkpoint index; avoids wildcard keys and JsonElement casts. |
| Session persistence + resume/report samples (Sections 7.3, 8.6–8.8) | Verified with the typed `IStorage` contract | Uses explicit checkpoint index keys only; no wildcard reads. |
| AG-UI server wiring (Section 11.2) | Illustrative | Names may change between Microsoft.Agents.AI.Hosting.AGUI.AspNetCore previews; align with the templates shipped in your installed preview. |
| Agent workflow APIs (Sections 6–9) | Illustrative | WorkflowBuilder/WorkflowExecutor surface is still in flux; treat as patterns, not drop-in code. |
| CopilotKit integration (Sections 11.3–11.6) | Verified for CopilotKit 1.50.x | Matches published CopilotKit examples for headers/runtimeUrl/tooling. |
| Observability + Prometheus queries (Section 10) | Verified for OpenTelemetry 1.14 + ASP.NET Core default metrics | Uses `http_server_request_duration_seconds_*` instruments and OTLP to Loki with a single limits block. |
| Authentication cookie + rate limits (Section 12) | Verified for ASP.NET Core 10 | SameSite=Lax/None for OAuth, partition keys by user id/IP, not Host header. |

*End of Document*
var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var postgres = builder.AddPostgres("postgres").WithPgAdmin()
    .WithDataVolume("dbserver-volume", isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("myappdb");

var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
    .WithDataVolume("rabbitmq-data", isReadOnly: false)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var docling = builder.AddContainer("docling", "quay.io/docling-project/docling-serve-cu124:latest")
    .WithArgs("sh", "-c", "docling-tools models download --output-dir /opt/app-root/src/.cache/docling/models --all && exec docling-serve run")
    .WithEnvironment("DOCLING_SERVE_ENABLE_UI", "true")
    .WithEnvironment("UVICORN_WORKERS", "1")
    .WithEnvironment("DOCLING_SERVE_ARTIFACTS_PATH", "/opt/app-root/src/.cache/docling/models")
    .WithEnvironment("DOCLING_SERVE_LOAD_MODELS_AT_BOOT", "true")
    .WithEnvironment("TRANSFORMERS_VERBOSITY", "info")
    .WithEnvironment("DOCLING_SERVE_MAX_SYNC_WAIT", "900")
    .WithVolume("docling-models-volume", "/opt/app-root/src/.cache/docling/models", isReadOnly: false)
    .WithHttpEndpoint(port: 8001, targetPort: 5001, name: "docling-api")
    .WithLifetime(ContainerLifetime.Persistent);

var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume("qdrant-data", isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.MyApp>("myapp")
    .WaitFor(docling)
    .WithReference(database).WaitFor(database)
    .WithReference(qdrant).WaitFor(qdrant);

await builder.Build().RunAsync();

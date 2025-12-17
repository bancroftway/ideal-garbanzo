var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyApp_Server_WebApi>("myapp-server-webapi");

builder.Build().Run();

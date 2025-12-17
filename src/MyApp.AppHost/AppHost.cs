var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyApp>("myapp");

builder.AddProject<Projects.MyApp_Web>("myapp-web");

builder.Build().Run();

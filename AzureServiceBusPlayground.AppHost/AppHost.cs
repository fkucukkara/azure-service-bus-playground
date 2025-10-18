var builder = DistributedApplication.CreateBuilder(args);

var asbConnString = builder.AddConnectionString("azure-service-bus");

var apiService = builder.AddProject<Projects.AzureServiceBusPlayground_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(asbConnString);

builder.AddProject<Projects.AzureServiceBusPlayground_Notifications>("notifications")
    .WithReference(asbConnString);

builder.Build().Run();

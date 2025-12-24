using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("azure-service-bus");

// Use emulator in Development environment, cloud in Production
if (builder.Environment.IsDevelopment())
{
    serviceBus.RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));
}

serviceBus.AddServiceBusQueue("orders")
    .WithProperties(queue =>
    {
        queue.MaxDeliveryCount = 5;
    });

var topic = serviceBus.AddServiceBusTopic("order-events");
topic.AddServiceBusSubscription("notifications")
    .WithProperties(subscription =>
    {
        subscription.MaxDeliveryCount = 5;
    });

builder.AddProject<Projects.AzureServiceBusPlayground_ApiService>("api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.AddProject<Projects.AzureServiceBusPlayground_Notifications>("notification-service")
    .WithHttpHealthCheck("/health")
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.Build().Run();

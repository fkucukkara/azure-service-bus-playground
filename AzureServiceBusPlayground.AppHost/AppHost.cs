var builder = DistributedApplication.CreateBuilder(args);

// Used for reaching cloud Azure Service Bus instance
//var asbConnString = builder.AddConnectionString("azure-service-bus");

var serviceBus = builder.AddAzureServiceBus("azure-service-bus")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));


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
    .WithHttpsEndpoint(5001, name: "public")
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.AddProject<Projects.AzureServiceBusPlayground_Notifications>("notification-service")
    .WithHttpHealthCheck("/health")
    .WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.Build().Run();

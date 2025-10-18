using Azure.Messaging.ServiceBus;
using AzureServiceBusPlayground.Notifications.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<ServiceBusClient>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("azure-service-bus")
        ?? throw new InvalidOperationException("Service Bus connection string is not configured.");

    return new ServiceBusClient(connectionString);
});

builder.Services.AddHostedService<OrderNotificationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.Run();

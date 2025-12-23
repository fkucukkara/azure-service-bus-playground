using AzureServiceBusPlayground.Notifications.Services;

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Used for reaching cloud Azure Service Bus instance
//builder.Services.AddSingleton<ServiceBusClient>(_ =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("azure-service-bus")
//        ?? throw new InvalidOperationException("Service Bus connection string is not configured.");

//    return new ServiceBusClient(connectionString);
//});

builder.AddAzureServiceBusClient("azure-service-bus");

builder.Services.AddHostedService<OrderNotificationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.Run();

using AzureServiceBusPlayground.ApiService.Models;
using AzureServiceBusPlayground.ApiService.Services;
using AzureServiceBusPlayground.Domain;
using Microsoft.AspNetCore.Mvc;

AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

// Used for reaching cloud Azure Service Bus instance
//builder.Services.AddSingleton<ServiceBusClient>(_ =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("azure-service-bus")
//        ?? throw new InvalidOperationException("Service Bus connection string is not configured.");

//    return new ServiceBusClient(connectionString);
//});

builder.AddAzureServiceBusClient("azure-service-bus");

builder.Services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/orders", CreateOrderHandler)
    .WithName("CreateOrder")
    .WithOpenApi();

static async Task<IResult> CreateOrderHandler(
    [FromBody] CreateOrderRequest request,
    [FromServices] IServiceBusPublisher serviceBusPublisher,
    [FromServices] ILogger<Program> logger)
{
    try
    {
        var orderGuid = Guid.NewGuid();

        var orderEvent = new OrderCreatedEvent(
            orderGuid,
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            request.Items?.Select(item => new OrderItem(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.Price
            )).ToList() ?? [],
            request.TotalAmount,
            DateTime.UtcNow
        );

        await serviceBusPublisher.PublishOrderCreatedEventAsync(orderEvent);

        logger.LogInformation("OrderCreatedEvent published successfully for OrderId: {OrderGuid}", orderGuid);

        return TypedResults.Created($"/api/orders/{orderGuid}", new { OrderId = orderGuid });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while publishing OrderCreatedEvent");
        return TypedResults.Problem(
            title: "Failed to create order",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}

app.MapDefaultEndpoints();

app.Run();
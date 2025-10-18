using Azure.Messaging.ServiceBus;
using AzureServiceBusPlayground.Domain;
using System.Text.Json;

namespace AzureServiceBusPlayground.ApiService.Services;


public interface IServiceBusPublisher
{
    Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderEvent);
}

public class ServiceBusPublisher(ServiceBusClient serviceBusClient) : IServiceBusPublisher
{
    private const string QueueName = "orders";
    private const string TopicName = "order-events";

    public async Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderEvent)
    {
        var messageBody = JsonSerializer.Serialize(orderEvent);

        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            Subject = nameof(OrderCreatedEvent),
            MessageId = orderEvent.OrderId.ToString(),
            ApplicationProperties =
            {
                { "CustomerId", orderEvent.CustomerId.ToString() },
                { "CreatedDate", orderEvent.CreatedDate.ToString("o") },
                { "TotalAmount", orderEvent.TotalAmount }
            }
        };

        await using var queueSender = serviceBusClient.CreateSender(QueueName);
        await queueSender.SendMessageAsync(message);

        await using var topicSender = serviceBusClient.CreateSender(TopicName);
        await topicSender.SendMessageAsync(message);
    }
}

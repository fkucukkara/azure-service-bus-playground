
using Azure.Messaging.ServiceBus;
using AzureServiceBusPlayground.Domain;
using System.Text.Json;

namespace AzureServiceBusPlayground.Notifications.Services;

public class OrderNotificationService(
    ServiceBusClient serviceBusClient,
    ILogger<OrderNotificationService> logger) : BackgroundService
{
    private const string QueueName = "orders";
    private const string TopicName = "order-events";
    private const string SubscriptionName = "notifications";
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueTask = ConsumeFromQueueAsync(stoppingToken);
        var topicTask = ConsumeFromTopicAsync(stoppingToken);

        await Task.WhenAll(queueTask, topicTask);
    }

    private async Task ConsumeFromQueueAsync(CancellationToken ct)
    {
        await using var processor = serviceBusClient.CreateProcessor(
            QueueName,
            new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        processor.ProcessMessageAsync += args => ProcessMessageAync(args, "queue");
        processor.ProcessErrorAsync += args => ProcessErrorAsync(args);

        await processor.StartProcessingAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is stopping
        }

        await processor.StopProcessingAsync();
    }

    private async Task ConsumeFromTopicAsync(CancellationToken ct)
    {
        await using var processor = serviceBusClient.CreateProcessor(
            TopicName, SubscriptionName,
            new ServiceBusProcessorOptions()
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        processor.ProcessMessageAsync += args => ProcessMessageAync(args, "topic");
        processor.ProcessErrorAsync += args => ProcessErrorAsync(args);

        await processor.StartProcessingAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // Expected when the service is stopping
        }

        await processor.StopProcessingAsync();
    }

    private async Task ProcessMessageAync(ProcessMessageEventArgs eventArgs, string source)
    {
        try
        {
            var body = eventArgs.Message.Body.ToString();
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(body);

            if (orderEvent is not null)
            {
                logger.LogInformation("Processing {Source} message for OrderId: {OrderId}, Customer: {CustomerName}, TotalAmount: {TotalAmount}",
                    source,
                    orderEvent.OrderId,
                    orderEvent.CustomerName,
                    orderEvent.TotalAmount);
            }

            await eventArgs.CompleteMessageAsync(eventArgs.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message from {Type}", source);
            await eventArgs.AbandonMessageAsync(eventArgs.Message);
        }
    }

    private async Task ProcessErrorAsync(ProcessErrorEventArgs eventArgs)
    {
        logger.LogError(eventArgs.Exception, "Error processing message from Queue");

        await Task.CompletedTask;

    }
}

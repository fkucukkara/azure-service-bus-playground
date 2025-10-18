namespace AzureServiceBusPlayground.Domain;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    IEnumerable<OrderItem> Items,
    decimal TotalAmount,
    DateTime CreatedDate);
public record OrderItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price);
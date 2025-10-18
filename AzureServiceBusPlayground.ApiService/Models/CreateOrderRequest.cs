namespace AzureServiceBusPlayground.ApiService.Models;

public record CreateOrderRequest(
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    IEnumerable<OrderItemRequest> Items,
    decimal TotalAmount);

public record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price);

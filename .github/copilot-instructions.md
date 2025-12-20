# Azure Service Bus Playground - AI Agent Instructions

## Architecture Overview

This is a **.NET Aspire** distributed application demonstrating Azure Service Bus patterns (queues + topics/subscriptions). The orchestrator is `AppHost.cs`, which wires services with shared connection strings.

### Service Boundaries
- **ApiService**: HTTP API that publishes `OrderCreatedEvent` to BOTH a queue (`orders`) and topic (`order-events`)
- **Notifications**: Background processor consuming from both the `orders` queue and `order-events` topic subscription
- **Domain**: Shared event contracts (`OrderCreatedEvent`, `OrderItem`)
- **ServiceDefaults**: Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience)
- **AppHost**: .NET Aspire orchestrator - manages service lifecycle and injects connection strings

### Data Flow
1. POST to `/api/orders` → ApiService receives `CreateOrderRequest`
2. Maps to domain `OrderCreatedEvent` with generated `OrderId`
3. **Dual publish**: ServiceBusPublisher sends to BOTH queue and topic atomically
4. Notifications service processes messages from both sources concurrently (separate processors)

### Key Architectural Decisions
- **Dual publish pattern**: Same message sent to queue AND topic for demonstrating both patterns
- **Primary key constructors**: All services use C# 12 primary constructors
- **Record types**: Domain events and DTOs are immutable records
- **Scoped publisher**: `IServiceBusPublisher` is scoped; `ServiceBusClient` is singleton
- **.NET 9 + Aspire 9.5.0**: Uses latest SDK features (see `global.json`)

## Project-Specific Conventions

### Messaging Patterns
```csharp
// Message metadata convention (from ServiceBusPublisher.cs)
var message = new ServiceBusMessage(jsonBody)
{
    ContentType = "application/json",
    Subject = nameof(OrderCreatedEvent),  // Event type as Subject
    MessageId = orderEvent.OrderId.ToString(),  // Domain ID as MessageId
    ApplicationProperties = { ... }  // Key domain properties for routing/filtering
};
```

### Entity Names
- Queue: `orders`
- Topic: `order-events`
- Subscription: `notifications`

*Note: These must exist in Azure Service Bus before running. Not created by code.*

### Service Configuration
Connection string injection via Aspire:
```csharp
// AppHost.cs pattern
var asbConnString = builder.AddConnectionString("azure-service-bus");
builder.AddProject<...>("servicename").WithReference(asbConnString);
```

Services retrieve via:
```csharp
builder.Configuration.GetConnectionString("azure-service-bus")
```

### Message Processing
- **AutoCompleteMessages = false**: Manual completion/abandonment for error handling
- **MaxConcurrentCalls = 1**: Sequential processing (educational constraint)
- Processors started in parallel tasks (`ConsumeFromQueueAsync` + `ConsumeFromTopicAsync`)

## Developer Workflows

### Local Development
```powershell
# Run Aspire AppHost (starts all services with dashboard)
dotnet run --project AzureServiceBusPlayground.AppHost

# Or individual services
dotnet run --project AzureServiceBusPlayground.ApiService
dotnet run --project AzureServiceBusPlayground.Notifications
```

### Prerequisites
- Azure Service Bus namespace with:
  - Queue: `orders`
  - Topic: `order-events` with subscription: `notifications`
- Connection string in `AzureServiceBusPlayground.AppHost/appsettings.Development.json`:
  ```json
  {
    "ConnectionStrings": {
      "azure-service-bus": "Endpoint=sb://..."
    }
  }
  ```

### Testing Message Flow
```powershell
# Send test order
curl -X POST http://localhost:PORT/api/orders `
  -H "Content-Type: application/json" `
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "items": [
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        "productName": "Widget",
        "quantity": 2,
        "price": 19.99
      }
    ],
    "totalAmount": 39.98
  }'

# Observe logs in Notifications service - same message processed twice (queue + topic)
```

## Integration Points

### External Dependencies
- **Azure.Messaging.ServiceBus**: All messaging operations
- **Aspire.Hosting.AppHost**: Orchestration SDK
- **OpenTelemetry**: Tracing/metrics via ServiceDefaults

### Cross-Component Communication
- **Domain events**: `AzureServiceBusPlayground.Domain` referenced by all projects
- **ServiceDefaults**: Extension method `AddServiceDefaults()` configures telemetry, health checks, resilience
- **No direct service-to-service calls**: Pure async messaging

### Health Endpoints
All services expose:
- `/health` - full health check
- `/alive` - liveness probe
- Configured in `ServiceDefaults/Extensions.cs`

## Common Tasks

### Adding a New Event Type
1. Define record in `Domain/Events.cs`
2. Add publish method to `IServiceBusPublisher`
3. Create new queue/topic in Azure
4. Add processor in consumer service (or create new service)

### Adding a New Consumer
1. Reference `Domain` + `ServiceDefaults` projects
2. Inject `ServiceBusClient` as singleton
3. Create `BackgroundService` with `ServiceBusProcessor`
4. Register in AppHost with `.WithReference(asbConnString)`

### Debugging Message Processing
- Check Aspire dashboard for distributed traces
- Notifications service logs show source (`queue` or `topic`)
- Use Azure Service Bus Explorer in portal for dead-letter inspection

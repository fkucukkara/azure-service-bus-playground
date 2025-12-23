# Azure Service Bus Playground

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C# 14](https://img.shields.io/badge/C%23-14-239120)](https://learn.microsoft.com/dotnet/csharp/)
[![Aspire 13.1](https://img.shields.io/badge/Aspire-13.1-5C2D91)](https://learn.microsoft.com/dotnet/aspire/)

An educational playground for learning Azure Service Bus messaging patterns using **.NET Aspire**. This project demonstrates real-world scenarios including **queues**, **topics/subscriptions**, and the **dual publish pattern** in a distributed microservices architecture.

## 🎯 What You'll Learn

- **Queue vs Topic messaging patterns** and when to use each
- **Dual publish pattern** - sending the same message to both queue and topic atomically
- Building distributed applications with **.NET Aspire** orchestration
- **Event-driven architecture** with domain events
- Message processing with **manual completion** and error handling
- **Local development** with Azure Service Bus Emulator (no Azure subscription required!)
- Modern C# features: **primary constructors**, **record types**, and **top-level statements**
- Observability with **OpenTelemetry** tracing and metrics

## 🏗️ Architecture

This is a **.NET Aspire distributed application** with the following services:

### Services

| Service | Purpose | Technology |
|---------|---------|------------|
| **ApiService** | HTTP API that publishes order events to both queue and topic | ASP.NET Core Web API |
| **Notifications** | Background processor consuming from both queue and topic subscription | .NET Worker Service |
| **Domain** | Shared event contracts (`OrderCreatedEvent`, `OrderItem`) | Class Library |
| **ServiceDefaults** | Aspire service defaults (OpenTelemetry, health, resilience) | Class Library |
| **AppHost** | .NET Aspire orchestrator managing service lifecycle and configuration | Aspire AppHost |

### Message Flow

```
POST /api/orders
       ↓
   ApiService
       ↓
   ServiceBusPublisher (Dual Publish)
       ├─────────────────┬─────────────────┐
       ↓                 ↓                 ↓
  Queue: orders    Topic: order-events    
       ↓                 ↓                 
       └─────────────────┴─────────────────┘
                    ↓
            Notifications Service
         (Processes from both sources)
```

### Key Architectural Decisions

- **Dual Publish Pattern**: Same message sent to both queue AND topic for demonstrating different consumption patterns
- **Primary Constructors**: All services use C# 14 primary constructors for DI
- **Record Types**: Domain events and DTOs are immutable records
- **Scoped Publisher**: `IServiceBusPublisher` is scoped; `ServiceBusClient` is singleton
- **Manual Message Completion**: `AutoCompleteMessages = false` for explicit error handling
- **Sequential Processing**: `MaxConcurrentCalls = 1` (educational constraint - increase for production)

## ✨ Features

- ✅ **Dual Publish Pattern**: Send same message to queue and topic atomically
- ✅ **Queue Processing**: Demonstrates point-to-point messaging
- ✅ **Topic/Subscription Processing**: Demonstrates pub-sub messaging
- ✅ **Event-Driven Architecture**: Decoupled services communicating via domain events
- ✅ **Aspire Orchestration**: Automatic service discovery, configuration, and dashboard
- ✅ **Service Bus Emulator**: Local development without Azure subscription using containerized emulator
- ✅ **Auto-Provisioned Entities**: Queue, topic, and subscription created automatically by Aspire
- ✅ **OpenTelemetry Integration**: Built-in distributed tracing and metrics
- ✅ **Health Checks**: `/health` and `/alive` endpoints on all services
- ✅ **Error Handling**: Manual message completion with abandon/dead-letter patterns
- ✅ **Modern C#**: Primary constructors, record types, global usings

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK or later](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (required for Azure Service Bus Emulator)
- [Visual Studio 2022 Preview](https://visualstudio.microsoft.com/vs/preview/) or [Visual Studio Code](https://code.visualstudio.com/) with [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [Azure Subscription](https://azure.microsoft.com/free/) (optional - only needed if connecting to cloud Service Bus)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (optional - only needed for cloud setup)

### Azure Service Bus Setup

#### Default: Local Emulator (Recommended for Development)

By default, this project uses the **Azure Service Bus Emulator** running in Docker. **No Azure subscription or manual setup is required!**

The following entities are **automatically provisioned** by Aspire when you run the application:

| Entity Type | Name | Description | Configuration |
|-------------|------|-------------|---------------|
| Queue | `orders` | Receives order created events | MaxDeliveryCount = 5 |
| Topic | `order-events` | Broadcasts order events to subscribers | - |
| Subscription | `notifications` | Under `order-events` topic | MaxDeliveryCount = 5 |

The emulator container runs with **persistent lifetime**, so your data survives restarts.

#### Alternative: Cloud Azure Service Bus

To use a cloud Azure Service Bus instance instead of the emulator:

1. **Create Azure resources** using Azure Portal or CLI:

   ```bash
   # Variables
   RESOURCE_GROUP="rg-servicebus-playground"
   LOCATION="eastus"
   NAMESPACE="sb-playground-$(openssl rand -hex 4)"

   # Create resources
   az group create --name $RESOURCE_GROUP --location $LOCATION
   az servicebus namespace create --name $NAMESPACE --resource-group $RESOURCE_GROUP --sku Standard
   az servicebus queue create --name orders --namespace-name $NAMESPACE --resource-group $RESOURCE_GROUP
   az servicebus topic create --name order-events --namespace-name $NAMESPACE --resource-group $RESOURCE_GROUP
   az servicebus topic subscription create --name notifications --topic-name order-events --namespace-name $NAMESPACE --resource-group $RESOURCE_GROUP

   # Get connection string
   az servicebus namespace authorization-rule keys list \
     --resource-group $RESOURCE_GROUP \
     --namespace-name $NAMESPACE \
     --name RootManageSharedAccessKey \
     --query primaryConnectionString -o tsv
   ```

2. **Update AppHost.cs** to use the connection string instead of emulator:

   ```csharp
   // Comment out the emulator configuration:
   // var serviceBus = builder.AddAzureServiceBus("azure-service-bus")
   //     .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

   // Use cloud connection string instead:
   var asbConnString = builder.AddConnectionString("azure-service-bus");
   ```

3. **Add connection string** to `AzureServiceBusPlayground.AppHost/appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "azure-service-bus": "Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY"
     }
   }
   ```

### Configuration

1. **Clone the repository**
   ```bash
   git clone https://github.com/fkucukkara/azure-service-bus-playground.git
   cd AzureServiceBusPlayground
   ```

2. **Ensure Docker is running**
   
   The Azure Service Bus Emulator runs as a Docker container. Make sure Docker Desktop is running before starting the application.

3. **Build the solution**
   ```bash
   dotnet build
   ```

> **Note**: No connection string configuration is needed for local development! The emulator is automatically started and configured by Aspire.

### Running the Application

**Option 1: Run with Aspire AppHost (Recommended)**

This starts all services with the Aspire dashboard for observability:

```bash
dotnet run --project AzureServiceBusPlayground.AppHost
```

The Aspire dashboard will open automatically, showing:
- Service status and logs
- Distributed traces
- Metrics
- Resource management

**Option 2: Run services individually**

In separate terminals:

```bash
# Terminal 1 - API Service
dotnet run --project AzureServiceBusPlayground.ApiService

# Terminal 2 - Notifications Service
dotnet run --project AzureServiceBusPlayground.Notifications
```

## 📝 Usage Examples

### Send a Test Order

**Using curl (PowerShell):**

```powershell
curl -X POST http://localhost:5000/api/orders `
  -H "Content-Type: application/json" `
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "items": [
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        "productName": "Widget Pro",
        "quantity": 2,
        "price": 29.99
      },
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
        "productName": "Gadget Ultra",
        "quantity": 1,
        "price": 49.99
      }
    ],
    "totalAmount": 109.97
  }'
```

**Using curl (bash/Linux/macOS):**

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "items": [
      {
        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
        "productName": "Widget Pro",
        "quantity": 2,
        "price": 29.99
      }
    ],
    "totalAmount": 59.98
  }'
```

**Using the .http file:**

Open `AzureServiceBusPlayground.ApiService/AzureServiceBusPlayground.ApiService.http` in Visual Studio or VS Code with REST Client extension and click "Send Request".

### Expected Behavior

When you send an order, you'll see:

1. **ApiService** logs showing message published to both queue and topic
2. **Notifications service** logs showing **two** message receptions:
   - One from the `orders` queue
   - One from the `order-events` topic subscription
3. **Aspire dashboard** showing distributed traces connecting the publish and consume operations

Example log output:
```
[Notifications] Received message from queue: Order {OrderId} for customer {CustomerName}
[Notifications] Received message from topic: Order {OrderId} for customer {CustomerName}
```

## 🎓 Educational Goals

### Messaging Patterns

- **Queue (Point-to-Point)**: Single consumer receives each message. Use for:
  - Task distribution among workers
  - Load balancing
  - Guaranteed single processing
  
- **Topic/Subscription (Pub-Sub)**: Multiple subscribers can receive each message. Use for:
  - Broadcasting events to multiple services
  - Event notification patterns
  - Decoupled microservices

- **Dual Publish Pattern**: Send to both queue and topic. Use for:
  - Guaranteed processing (queue) + optional subscriptions (topic)
  - Hybrid scenarios where one consumer must process, others may observe
  - Migration scenarios (gradual transition from queue to pub-sub)

### Message Metadata Convention

This project uses a consistent pattern for message metadata:

```csharp
var message = new ServiceBusMessage(jsonBody)
{
    ContentType = "application/json",
    Subject = nameof(OrderCreatedEvent),  // Event type
    MessageId = orderEvent.OrderId.ToString(),  // Domain ID
    ApplicationProperties = {
        ["CustomerId"] = orderEvent.CustomerId,
        ["TotalAmount"] = orderEvent.TotalAmount,
        ["ItemCount"] = orderEvent.Items.Count
    }
};
```

### Best Practices Demonstrated

- ✅ **Connection management**: Singleton `ServiceBusClient`, scoped publishers/processors
- ✅ **Manual message completion**: Explicit control over success/failure handling
- ✅ **Structured logging**: Correlation IDs and contextual information
- ✅ **Health checks**: Endpoints for container orchestration
- ✅ **Graceful shutdown**: Proper disposal of Service Bus resources
- ✅ **Configuration management**: Aspire-based connection string injection
- ✅ **Observability**: OpenTelemetry tracing and metrics
- ✅ **Error handling**: Try-catch with message abandonment on failure

## 🛠️ Troubleshooting

### Emulator Issues

**Problem**: Emulator container fails to start

**Solutions**:
- Ensure Docker Desktop is running
- Check Docker has enough resources allocated (memory/CPU)
- Try removing old emulator containers: `docker rm -f $(docker ps -aq --filter "name=servicebus")`
- Check Aspire dashboard for container logs

**Problem**: `Connection refused` or timeout connecting to emulator

**Solutions**:
- Wait a few seconds after startup - the emulator needs time to initialize
- Check the Aspire dashboard to see if the emulator container is healthy
- Verify no other process is using the emulator's ports

### Connection Issues (Cloud Service Bus)

**Problem**: `ServiceBusException: The operation was aborted`

**Solutions**:
- Verify connection string in `appsettings.Development.json`
- Check that your Service Bus namespace exists and is accessible
- Ensure queue `orders`, topic `order-events`, and subscription `notifications` are created
- Verify firewall rules if using IP restrictions

### Message Not Received

**Problem**: Messages sent but not appearing in logs

**Solutions**:
- Check Notifications service is running
- Verify entity names match exactly: `orders`, `order-events`, `notifications`
- Check dead-letter queues in Azure Portal for failed messages
- Increase logging level in `appsettings.Development.json`:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Azure.Messaging.ServiceBus": "Debug"
      }
    }
  }
  ```

### Duplicate Processing

**Problem**: Same message processed twice

**Expected Behavior**: This is intentional! The dual publish pattern sends to both queue and topic, so Notifications service processes it twice (once from each source). Check logs for `Received message from queue` vs `Received message from topic`.

### Aspire Dashboard Not Opening

**Problem**: Dashboard doesn't launch automatically

**Solutions**:
- Check console output for dashboard URL (usually `http://localhost:15000`)
- Ensure port 15000 is not in use
- Try accessing manually: `http://localhost:15000`

### Build Errors

**Problem**: `The type or namespace name 'Aspire' could not be found`

**Solutions**:
- Ensure .NET 10.0 SDK is installed: `dotnet --version`
- Run `dotnet restore` at solution root
- Check `global.json` for correct SDK version
- Clean and rebuild: `dotnet clean && dotnet build`

## 🔍 Monitoring and Observability

### Aspire Dashboard

Access at `http://localhost:15000` (when running AppHost):

- **Traces**: View distributed traces across services
- **Logs**: Structured logs with filters
- **Metrics**: Request rates, durations, errors
- **Resources**: Service health and configuration

### Health Endpoints

All services expose:
- `/health` - Full health check (includes dependencies)
- `/alive` - Simple liveness probe

Test with:
```bash
curl http://localhost:5000/health
curl http://localhost:5000/alive
```

### Azure Service Bus Explorer

In Azure Portal:
1. Navigate to your Service Bus namespace
2. Select queue/topic
3. Click "Service Bus Explorer"
4. View messages, peek, dead-letters, and resend messages

## 🧪 Testing Scenarios

### Scenario 1: Basic Message Flow
1. Send order via API
2. Verify dual processing in Notifications logs
3. Check Aspire dashboard for distributed trace

### Scenario 2: Error Handling
1. Stop Notifications service
2. Send multiple orders
3. Messages accumulate in queue/subscription
4. Start Notifications - messages processed automatically

### Scenario 3: Dead Letter
1. Send malformed JSON directly to queue (using Azure Portal)
2. Observe message moved to dead-letter queue
3. Inspect dead-letter queue in Service Bus Explorer

### Scenario 4: Message Metadata
1. Send order with multiple items
2. Check message properties in Azure Portal Service Bus Explorer
3. Verify `Subject`, `MessageId`, and `ApplicationProperties`

## 🚀 Next Steps

### Extend the Project

1. **Add More Event Types**
   - Create `OrderCancelledEvent`, `OrderShippedEvent`
   - Demonstrate different message processing paths

2. **Add Another Consumer**
   - Create an `Analytics` service subscribing to the topic
   - Process events differently than Notifications

3. **Implement Message Filtering**
   - Add SQL filters to topic subscriptions
   - Route high-value orders to special subscription

4. **Add Dead Letter Handling**
   - Create service to process dead-letter messages
   - Implement retry logic with exponential backoff

5. **Session Support**
   - Enable sessions on queue
   - Implement stateful processing per customer

6. **Production Patterns**
   - Increase `MaxConcurrentCalls` for throughput
   - Implement circuit breaker for resilience
   - Add correlation ID propagation
   - Implement idempotency checks

### Production Checklist

Before deploying to production:

- [ ] Use Managed Identity instead of connection strings
- [ ] Configure appropriate `MaxConcurrentCalls` and `PrefetchCount`
- [ ] Implement proper dead-letter queue monitoring
- [ ] Set up alerts for queue depth and processing failures
- [ ] Enable duplicate detection if needed
- [ ] Configure message TTL and lock duration appropriately
- [ ] Implement retry policies with exponential backoff
- [ ] Use separate service principals per environment
- [ ] Enable diagnostic logs and metrics
- [ ] Test failover scenarios

## 📚 Resources

### Azure Service Bus
- [Azure Service Bus Documentation](https://learn.microsoft.com/azure/service-bus-messaging/)
- [Queues, Topics, and Subscriptions](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-queues-topics-subscriptions)
- [Service Bus Best Practices](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
- [Azure Service Bus Emulator](https://learn.microsoft.com/azure/service-bus-messaging/overview-emulator)

### .NET Aspire
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Service Defaults](https://learn.microsoft.com/dotnet/aspire/fundamentals/service-defaults)
- [Aspire Dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard)

### Microsoft Learn Training
- [Implement message-based communication workflows with Azure Service Bus](https://learn.microsoft.com/training/modules/implement-message-workflows-with-service-bus/)
- [Choose a messaging model in Azure](https://learn.microsoft.com/training/modules/choose-a-messaging-model-in-azure/)

### C# and .NET
- [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Primary Constructors](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/primary-constructors)
- [Record Types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Commit your changes**: `git commit -m 'Add amazing feature'`
4. **Push to the branch**: `git push origin feature/amazing-feature`
5. **Open a Pull Request**

### Contribution Ideas
- Add new event types and scenarios
- Improve error handling patterns
- Add integration tests
- Enhance documentation
- Add Bicep/Terraform templates for infrastructure
- Create Docker Compose setup
- Add GitHub Actions CI/CD pipeline

## 📄 License

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

This project is licensed under the MIT License. See the [`LICENSE`](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/)
- Messaging powered by [Azure Service Bus](https://azure.microsoft.com/services/service-bus/)
- Inspired by real-world microservices architectures

---

**Made with ❤️ for learning Azure Service Bus patterns**

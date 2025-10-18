# AzureServiceBusPlayground

This project is an educational playground for learning and experimenting with Azure Service Bus, focusing on both Queues and Topics. It demonstrates how to build distributed .NET applications that communicate using Azure Service Bus messaging patterns.

## Project Structure

- **AzureServiceBusPlayground.ApiService**: ASP.NET Core Web API for sending messages to Azure Service Bus (Queues/Topics).
- **AzureServiceBusPlayground.AppHost**: Console or background service for processing messages from Azure Service Bus.
- **AzureServiceBusPlayground.Notifications**: Service for handling notifications, potentially subscribing to topics.
- **AzureServiceBusPlayground.Domain**: Shared domain models and event definitions.
- **AzureServiceBusPlayground.ServiceDefaults**: Shared service configuration and extension methods.

## Features

- **Send and Receive Messages**: Demonstrates sending messages to queues and topics, and receiving them in different services.
- **Event-Driven Architecture**: Uses domain events to decouple services.
- **Configuration via appsettings.json**: All connection strings and settings are managed via configuration files.
- **Educational Comments**: Code is commented to help you understand the flow and best practices.

## Getting Started

### Prerequisites
- [.NET 7.0 SDK or later](https://dotnet.microsoft.com/download)
- [Azure Subscription](https://azure.microsoft.com/free/)
- [Azure Service Bus Namespace](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-create-namespace-portal)

### Setup
1. **Clone the repository**
   ```sh
   git clone https://github.com/yourusername/AzureServiceBusPlayground.git
   cd AzureServiceBusPlayground
   ```
2. **Configure Azure Service Bus**
   - Create a Service Bus namespace in Azure.
   - Create a Queue and/or Topic + Subscription as needed.
   - Update `appsettings.json` in each project with your Service Bus connection string and entity names.

3. **Build the solution**
   ```sh
   dotnet build
   ```

4. **Run the API Service**
   ```sh
   dotnet run --project AzureServiceBusPlayground.ApiService
   ```

5. **Run the AppHost and Notifications services** (in separate terminals):
   ```sh
   dotnet run --project AzureServiceBusPlayground.AppHost
   dotnet run --project AzureServiceBusPlayground.Notifications
   ```

## Usage
- Use the API endpoints in `AzureServiceBusPlayground.ApiService` to send messages.
- Observe how messages are processed by the AppHost and Notifications services.
- Experiment with different messaging patterns (queues vs. topics).

## Educational Goals
- Understand the difference between queues and topics in Azure Service Bus.
- Learn how to structure distributed .NET applications using messaging.
- Practice best practices for configuration, dependency injection, and event-driven design.

## Resources
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Microsoft Learn: Azure Service Bus](https://learn.microsoft.com/training/modules/introduction-azure-service-bus/)

## License
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

This project is licensed under the MIT License. See the [`LICENSE`](LICENSE) file for details.

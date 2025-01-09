# Notification System

A microservices-based notification system that supports both email and webhook notifications using .NET 8, Kafka, and SendGrid.

## Project Structure

```
NotificationSystem/
├── src/
│   ├── NotificationService.API/        # REST API for receiving notifications
│   ├── NotificationProcessor.API/      # gRPC service for processing notifications
│   ├── NotificationProcessor.Worker/   # Background worker for handling notifications
│   └── NotificationService.Contracts/  # Shared models and contracts
├── docker-compose.yml
└── README.md
```

## Services

1. **NotificationService.API**: REST API that accepts notification requests and publishes them to Kafka
2. **NotificationProcessor.API**: gRPC service for notification processing
3. **NotificationProcessor.Worker**: Background worker service that consumes notifications from Kafka and processes them
4. **Kafka**: Message broker for asynchronous communication between services
5. **Kafka UI**: Web interface for monitoring Kafka

## Prerequisites

- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [SendGrid Account](https://sendgrid.com/) (for email notifications)

## Configuration

1. Create a `.env` file in the root directory:
```env
SENDGRID_API_KEY=your_sendgrid_api_key_here
```

2. Update the FROM_EMAIL in EmailNotificationHandler.cs with your SendGrid verified sender.

## Running the Application

1. Build and start all services:
```bash
docker-compose up --build
```

2. The following services will be available:
- Notification API: http://localhost:5000
- Notification Processor API: http://localhost:5002
- Kafka UI: http://localhost:8080

## Testing

1. Send an email notification:
```bash
curl -X POST http://localhost:5000/api/notification \
-H "Content-Type: application/json" \
-d '{
  "title": "Test Email",
  "content": "This is a test email",
  "type": 0,
  "priority": 1,
  "to": "recipient@example.com"
}'
```

2. Send a webhook notification:
```bash
curl -X POST http://localhost:5000/api/notification \
-H "Content-Type: application/json" \
-d '{
  "title": "Test Webhook",
  "content": "This is a test webhook notification",
  "type": 1,
  "priority": 1,
  "endpoint": "https://webhook.site/your-id",
  "traceId": "test-123"
}'
```

## Notification Types

1. **Email (type: 0)**
   - Required fields:
     - title: Email subject
     - content: Email body
     - to: Recipient email address

2. **Webhook (type: 1)**
   - Required fields:
     - title: Notification title
     - content: Notification content
     - endpoint: Webhook URL
     - traceId: Tracking ID

## Monitoring

1. **Logs**: View container logs
```bash
docker-compose logs -f [service-name]
```

2. **Kafka UI**: Monitor Kafka topics and messages
- Open http://localhost:8080 in your browser

## Stopping the Application

```bash
docker-compose down
```

To remove all containers and volumes:
```bash
docker-compose down -v
```

## Development

### Local Development Setup

1. Install dependencies:
```bash
cd src/[project-name]
dotnet restore
```

2. Run individual services:
```bash
dotnet run
```

### Project Dependencies

- .NET 8.0
- Confluent.Kafka
- SendGrid
- FluentValidation
- Swagger/OpenAPI

## Error Handling

The system implements comprehensive error handling:
- Validation errors return 400 Bad Request
- Processing errors return 500 Internal Server Error
- All errors are logged with appropriate context

## Architecture

```
┌─────────────────┐         ┌─────────────┐         ┌──────────────────┐
│  REST API       │         │             │         │  Worker Service   │
│  (Notifications)│ ──────► │    Kafka    │ ──────► │  (Email/Webhook) │
└─────────────────┘         │             │         └──────────────────┘
                            └─────────────┘
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details
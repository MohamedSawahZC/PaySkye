using Confluent.Kafka;
using NotificationProcessor.Worker.Handlers;
using NotificationProcessor.Worker.Services;
using NotificationService.API.Infrastructure;
using SendGrid;

var builder = Host.CreateApplicationBuilder(args);

// Add Kafka Configuration
var kafkaConfig = builder.Configuration.GetSection("Kafka").Get<KafkaConfiguration>()
    ?? new KafkaConfiguration();
builder.Services.AddSingleton(kafkaConfig);

// Configure Kafka Consumer
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = kafkaConfig.BootstrapServers,
        GroupId = "notification-processor",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

// Add notification handlers as scoped
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();
builder.Services.AddScoped<INotificationHandler, WebhookNotificationHandler>();

// Add HttpClient for webhook handler
builder.Services.AddHttpClient();

// Add SendGrid client
builder.Services.AddSingleton<ISendGridClient>(new SendGridClient("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"));

// Add hosted service
builder.Services.AddHostedService<NotificationConsumerService>();

var host = builder.Build();
host.Run();
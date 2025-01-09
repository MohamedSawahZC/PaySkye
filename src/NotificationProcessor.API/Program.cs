using Confluent.Kafka;
using Microsoft.OpenApi.Models;
using NotificationProcessor.API.Models;
using NotificationProcessor.API.Services;
using NotificationService.API.Infrastructure;
using NotificationService.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Processor API",
        Version = "v1",
        Description = "API for processing notifications"
    });
});

// Add Kafka Configuration
var kafkaConfig = builder.Configuration.GetSection("Kafka").Get<KafkaConfiguration>()
    ?? new KafkaConfiguration();
builder.Services.AddSingleton(kafkaConfig);

// Configure Kafka Producer
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = kafkaConfig.BootstrapServers,
        EnableDeliveryReports = true,
        ClientId = "notification-processor"
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// Add services
builder.Services.AddScoped<INotificationProducer, KafkaNotificationProducer>();
builder.Services.AddScoped<INotificationGrpcService, NotificationGrpcService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Processor API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
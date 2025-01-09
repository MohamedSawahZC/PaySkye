using Confluent.Kafka;
using FluentValidation;
using NotificationService.API.Infrastructure;
using NotificationService.API.Services;
using NotificationService.API.Validators;
using NotificationService.Contracts.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        ClientId = "notification-api"
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// Add services
builder.Services.AddScoped<INotificationProducer, KafkaNotificationProducer>();
builder.Services.AddScoped<IValidator<NotificationRequest>, NotificationRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
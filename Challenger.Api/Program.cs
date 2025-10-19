using Challenger.Infra.Data;
using Challenger.Domain.Repositories;
using Challenger.Infra.Repositories;
using Challenger.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Challenger.App.Messaging.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Challenger API",
        Version = "v1",
        Description = "API for managing couriers and motorcycles"
    });

    // Enable [SwaggerOperation] and other annotations (optional but useful)
    c.EnableAnnotations();
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseNpgsql(connectionString));

// DI registrations
builder.Services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
builder.Services.AddScoped<IMotorcycleService, MotorcycleService>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
builder.Services.AddScoped<IRentalService, RentalService>();

// Storage provider selection
var storageProvider = builder.Configuration["Storage:Provider"] ?? "Local";
if (string.Equals(storageProvider, "Minio", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IStorageService, MinioStorageService>();
}
else
{
    builder.Services.AddSingleton<IStorageService, LocalStorageService>();
}
// Kafka (optional): only wire if enabled and configured
var kafkaSettings = builder.Configuration.GetSection("Kafka").Get<KafkaSettings>() ?? new KafkaSettings();
if (kafkaSettings.Enabled && !string.IsNullOrWhiteSpace(kafkaSettings.BootstrapServers))
{
    builder.Services.AddSingleton<Challenger.App.Services.IEventProducer, Challenger.App.Messaging.Kafka.KafkaProducer>();
    builder.Services.AddHostedService<Challenger.App.Messaging.Kafka.MotorcycleCreatedConsumer>();
}
else
{
    // Fallback to a no-op producer so services can resolve IEventProducer
    builder.Services.AddSingleton<Challenger.App.Services.IEventProducer, Challenger.App.Services.NullEventProducer>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

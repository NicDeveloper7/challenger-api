using Challenger.App.Messaging.Events;
using Challenger.Domain.Models;
using Challenger.Infra.Data;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Challenger.App.Messaging.Config;

namespace Challenger.Api.Messaging.Kafka
{
    public class MotorcycleCreatedConsumer : BackgroundService
    {
        private readonly ILogger<MotorcycleCreatedConsumer> _logger;
        private readonly IServiceProvider _provider;
        private readonly KafkaSettings _settings;

        public MotorcycleCreatedConsumer(ILogger<MotorcycleCreatedConsumer> logger, IServiceProvider provider, IConfiguration configuration)
        {
            _logger = logger;
            _provider = provider;
            _settings = configuration.GetSection("Kafka").Get<KafkaSettings>() ?? new KafkaSettings();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BootstrapServers))
            {
                _logger.LogWarning("Kafka disabled or not configured. Consumer will not start.");
                return;
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_settings.TopicMotorcycleCreated);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value == null) continue;

                    var evt = JsonSerializer.Deserialize<MotorcycleCreatedEvent>(cr.Message.Value);
                    if (evt == null) continue;

                    if (evt.Year == 2024)
                    {
                        using var scope = _provider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<SystemDbContext>();

                        var exists = await db.HighlightedMotorcycles.AsNoTracking().AnyAsync(h => h.MotorcycleId == evt.Id, stoppingToken);
                        if (!exists)
                        {
                            db.HighlightedMotorcycles.Add(new HighlightedMotorcycle
                            {
                                Id = Guid.NewGuid(),
                                MotorcycleId = evt.Id,
                                Plate = evt.Plate,
                                Year = evt.Year,
                                Model = evt.Model,
                                HighlightedAt = DateTime.UtcNow
                            });
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Highlighted motorcycle saved for {Plate}", evt.Plate);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming motorcycle_created");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}


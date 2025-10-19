using Challenger.App.Messaging.Config;
using Challenger.App.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Challenger.App.Messaging.Kafka
{
    public class KafkaProducer : IEventProducer, IDisposable
    {
        private readonly IProducer<string, string>? _producer;
        private readonly ILogger<KafkaProducer> _logger;
        private readonly KafkaSettings _settings;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _logger = logger;
            _settings = configuration.GetSection("Kafka").Get<KafkaSettings>() ?? new KafkaSettings();

            if (!string.IsNullOrWhiteSpace(_settings.BootstrapServers))
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = _settings.BootstrapServers,
                    ClientId = _settings.ClientId ?? "challenger-api"
                };
                _producer = new ProducerBuilder<string, string>(config).Build();
            }
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            if (_producer == null)
            {
                _logger.LogDebug("Kafka producer disabled or not configured. Skipping publish to {Topic}.", topic);
                return;
            }

            var payload = JsonSerializer.Serialize(message);
            try
            {
                var dr = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = payload
                });
                _logger.LogInformation("Produced event to {Topic} at offset {Offset}", topic, dr.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed producing to {Topic}", topic);
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
        }
    }
}

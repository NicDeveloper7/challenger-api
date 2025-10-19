namespace Challenger.App.Messaging.Config
{
    public class KafkaSettings
    {
        // Gate to enable/disable Kafka integration at runtime
        public bool Enabled { get; set; } = false;
        public string BootstrapServers { get; set; } = "";
        public string ClientId { get; set; } = "challenger-app";
        public string TopicMotorcycleCreated { get; set; } = "motorcycle_created";
        public string ConsumerGroupId { get; set; } = "challenger-app-consumer";
    }
}

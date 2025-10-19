using System.Threading.Tasks;

namespace Challenger.App.Services
{
    public class NullEventProducer : IEventProducer
    {
        public Task ProduceAsync<T>(string topic, T message)
        {
            return Task.CompletedTask;
        }
    }
}

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tasky.Application.Interfaces;

namespace Tasky.Infrastructure.Services;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration.GetConnectionString("Kafka") ?? "localhost:9092";
        
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "Tasky.Api"
        };
        
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync<T>(string topic, string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json });
        }
        catch (ProduceException<string, string> e)
        {
            _logger.LogError(e, "Error producing to Kafka topic {Topic}: {Reason}", topic, e.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}

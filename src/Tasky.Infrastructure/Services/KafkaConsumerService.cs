using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Tasky.Application.Interfaces.Grains;
using Tasky.Domain.Entities;

namespace Tasky.Infrastructure.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IClusterClient _orleansClient;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, IClusterClient orleansClient)
    {
        _configuration = configuration;
        _logger = logger;
        _orleansClient = orleansClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration.GetConnectionString("Kafka") ?? "localhost:9092";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "Tasky.ConsumerGroup",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("task.comment.created");

        _logger.LogInformation("Kafka Consumer started, subscribing to task.comment.created");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result == null) continue;

                _logger.LogInformation("Consumed message from topic {Topic}: {Key}", result.Topic, result.Message.Key);

                if (result.Topic == "task.comment.created")
                {
                    var comment = JsonSerializer.Deserialize<TaskComment>(result.Message.Value);
                    if (comment != null)
                    {
                        var grain = _orleansClient.GetGrain<ITaskGrain>(comment.TaskId);
                        await grain.OnCommentAddedAsync(comment);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming Kafka message");
            }
        }

        consumer.Close();
    }
}

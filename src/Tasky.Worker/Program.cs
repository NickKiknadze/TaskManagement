using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using Tasky.Application.Interfaces.Grains;
using System.Text.Json;
using Tasky.Domain.Entities;

var builder = Host.CreateDefaultBuilder(args);

builder.UseOrleansClient(client =>
{
    client.UseLocalhostClustering();
});

builder.ConfigureServices((bot, services) =>
{
    services.AddHostedService<KafkaConsumerWorker>();
});

var host = builder.Build();
host.Run();

public class KafkaConsumerWorker : BackgroundService
{
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly IClusterClient _orleansClient;
    private readonly IConfiguration _configuration;
    private readonly string _topic = "task.comment.created"; 

    public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger, IClusterClient orleansClient, IConfiguration configuration)
    {
        _logger = logger;
        _orleansClient = orleansClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration.GetConnectionString("Kafka") ?? "localhost:9092",
            GroupId = "tasky-worker-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        
        try
        {
            consumer.Subscribe(_topic);
            _logger.LogInformation("Subscribed to {Topic}", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result != null)
                    {
                        await HandleMessageAsync(result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error creating consumer");
        }
        finally
        {
             consumer.Close();
        }
    }

    private async Task HandleMessageAsync(string messageValue)
    {
        try 
        {
             var comment = JsonSerializer.Deserialize<TaskComment>(messageValue);
             if (comment != null)
             {
                 var grain = _orleansClient.GetGrain<ITaskGrain>(comment.TaskId);
                 await grain.OnCommentAddedAsync(comment);
                 _logger.LogInformation("Processed comment for task {TaskId}", comment.TaskId);
             }
        }
        catch(Exception ex)
        {
             _logger.LogError(ex, "Failed to process message: {Message}", messageValue);
        }
    }
}

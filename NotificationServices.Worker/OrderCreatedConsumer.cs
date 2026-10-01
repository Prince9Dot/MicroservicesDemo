    using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Worker;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IConfiguration _configuration;

    public OrderCreatedConsumer(
        ILogger<OrderCreatedConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _logger.LogInformation("Connecting to RabbitMQ...");

        await using var connection =
            await factory.CreateConnectionAsync(stoppingToken);

        _logger.LogInformation(
            "RabbitMQ connection established.");

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMQ channel created.");

        await channel.QueueDeclareAsync(
            queue: "order_created_queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "RabbitMQ queue declared successfully.");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    ea.Body.ToArray());

                var orderEvent =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(
                        json);

                if (orderEvent == null)
                {
                    _logger.LogWarning(
                        "Received an invalid order event.");

                    await channel.BasicNackAsync(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                _logger.LogInformation(
                    "Notification: Order {OrderId} created. " +
                    "Product: {ProductId}, Quantity: {Quantity}, " +
                    "Total: {TotalAmount}",
                    orderEvent.OrderId,
                    orderEvent.ProductId,
                    orderEvent.Quantity,
                    orderEvent.TotalAmount);

                // Acknowledge only after successful processing.
                await channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Message processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process order-created message.");

                // Reject the message if processing fails.
                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        // IMPORTANT: Register the consumer OUTSIDE the event handler.
        await channel.BasicConsumeAsync(
            queue: "order_created_queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Consumer registered successfully. " +
            "Waiting for order-created messages.");

        // Keep the worker alive while waiting for messages.
        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "NotificationService.Worker is stopping.");
        }
    }
}
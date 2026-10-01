using OrderService.Application.Interfaces;
using RabbitMQ.Client;
using Shared.Contracts.Events;
using System.Text;
using System.Text.Json;


namespace OrderService.Infrastructure.Messaging
{
    public class RabbitMqOrderEventPublisher : IOrderEventPublisher
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqOrderEventPublisher(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;
        }

        public static async Task<RabbitMqOrderEventPublisher> CreateAsync(string hostName)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = "guest",
                Password = "guest"
            };

            var connection = await factory.CreateConnectionAsync();

            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                   queue : "order_created_queue",
                   durable: true,
                   exclusive: false,
                   autoDelete: false,
                   arguments: null
            );

            return new RabbitMqOrderEventPublisher(connection, channel);
        }
        public async Task PublishOrderCreatedAsync(OrderCreatedEvent orderCreatedEvent)
        {
            var json = JsonSerializer.Serialize(orderCreatedEvent);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: "order_created_queue",
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}

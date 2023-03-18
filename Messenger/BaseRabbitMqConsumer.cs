using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messenger;

public class BaseRabbitMqConsumer : BackgroundService
{
    private readonly RabbitMqConfiguration _config;
    private readonly string _consumerTag;
    protected readonly IModel Channel;
    protected readonly IConnection Connection;

    public BaseRabbitMqConsumer(IOptions<RabbitMqConfiguration> config)
    {
        _config = config.Value;

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_config.Uri),
            ClientProvidedName = _config.ClientProvidedName
        };

        Connection = factory.CreateConnection();
        Channel = Connection.CreateModel();

        Connection.ConnectionShutdown += OnConnectionShutdown;

        Channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Direct);
        Channel.QueueDeclare(_config.QueueName, false, false, false, null);
        Channel.QueueBind(_config.QueueName, _config.ExchangeName, _config.RoutingKey, null);
        Channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(Channel);
        consumer.Received += OnConsumerReceived;
        consumer.Shutdown += OnConsumerShutdown;
        consumer.Registered += OnConsumerRegistered;
        consumer.Unregistered += OnConsumerUnregistered;
        consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

        _consumerTag = Channel.BasicConsume(_config.QueueName, false, consumer);
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Delay(-1, cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Channel.BasicCancel(_consumerTag);
        Channel.Close();
        Connection.Close();

        return Task.CompletedTask;
    }

    protected virtual void OnConsumerReceived(object? sender, BasicDeliverEventArgs e)
    {
        Channel.BasicAck(e.DeliveryTag, false);
    }

    protected virtual void OnConsumerShutdown(object? sender, ShutdownEventArgs e)
    {
    }

    protected virtual void OnConsumerRegistered(object? sender, ConsumerEventArgs e)
    {
    }

    protected virtual void OnConsumerUnregistered(object? sender, ConsumerEventArgs e)
    {
    }

    protected virtual void OnConsumerConsumerCancelled(object? sender, ConsumerEventArgs e)
    {
    }

    protected virtual void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
    }
}
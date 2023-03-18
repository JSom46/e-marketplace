using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Messenger;

public class RabbitMqProducer : IMessageProducer
{
    private readonly IModel _channel;
    private readonly IConnection _con;
    private readonly RabbitMqConfiguration _config;

    public RabbitMqProducer(IOptions<RabbitMqConfiguration> config)
    {
        _config = config.Value;
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_config.Uri),
            ClientProvidedName = _config.ClientProvidedName
        };

        _con = factory.CreateConnection();
        _channel = _con.CreateModel();

        _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Direct);
        _channel.QueueDeclare(_config.QueueName, false, false, false);
        _channel.QueueBind(_config.QueueName, _config.ExchangeName, _config.RoutingKey, null);
    }

    public void SendMessage<T>(T message)
    {
        var messageBody = JsonSerializer.SerializeToUtf8Bytes(message);
        _channel.BasicPublish(_config.ExchangeName, _config.RoutingKey, null, messageBody);
    }
}
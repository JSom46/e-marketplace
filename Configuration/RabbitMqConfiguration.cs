namespace Messenger;

public class RabbitMqConfiguration
{
    public string Uri { get; set; }
    public string ClientProvidedName { get; set; }
    public string ExchangeName { get; set; }
    public string RoutingKey { get; set; }
    public string QueueName { get; set; }
}
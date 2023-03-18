namespace Messenger;

public interface IMessageProducer
{
    void SendMessage<T>(T message);
}
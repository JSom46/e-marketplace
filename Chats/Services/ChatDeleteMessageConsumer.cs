using System.Text.Json;
using Chats.DataAccess;
using Messenger;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;

namespace Chats.Services;

public class ChatDeleteMessageConsumer : BaseRabbitMqConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatDeleteMessageConsumer(IOptions<RabbitMqConfiguration> config, IServiceScopeFactory scopeFactory) : base(config)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async void OnConsumerReceived(object? sender, BasicDeliverEventArgs e)
    {
        var bodyArray = e.Body.ToArray();
        Guid announcementId;

        try
        {
            announcementId = JsonSerializer.Deserialize<Guid>(bodyArray);
        }
        catch (Exception)
        {
            // message is invalid, so let's get rid of it.
            Channel.BasicAck(e.DeliveryTag, false);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dataAccess = scope.ServiceProvider.GetRequiredService<IChatsDataAccess>();

        try
        {
            var chats = await dataAccess.GetByAnnouncementId(announcementId);

            foreach (var chat in chats)
            {
                Console.WriteLine($"deleting chat: {chat.Id}");
                await dataAccess.Delete(chat.Id);
            }

        }
        catch (Exception)
        {
            // error while deleting chats
            return;
        }

        // chat deleted
        Channel.BasicAck(e.DeliveryTag, false);
    }
}
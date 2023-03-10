namespace Chats.Hubs
{
    public interface IChatClient
    {
        Task NewMessage(Guid id);
        Task MessagesDelivered(IEnumerable<Guid> id);
    }
}

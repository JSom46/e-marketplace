namespace Chats.Hubs;

public interface IChatClient
{
    /// <summary>
    /// Informs about new message sent in chat.
    /// </summary>
    /// <param name="id">Id of new message.</param>
    Task NewMessage(Guid id);

    /// <summary>
    /// Informs about messages received by other chat user.
    /// </summary>
    /// <param name="ids">List of ids of messages.</param>
    Task MessagesDelivered(IEnumerable<Guid> ids);
}
namespace Chats.Models;

public class MessageAttachment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;
}
namespace Chats.Models;

public class Message
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedDate { get; set; }
    public bool Received { get; set; }
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
    public ICollection<MessageAttachment> MessageAttachments { get; } = new List<MessageAttachment>();
}
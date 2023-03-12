namespace Chats.Dtos;

public class GetChatResponseMessage
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public bool Received { get; set; }
    public IEnumerable<string> Attachments { get; set; }
}
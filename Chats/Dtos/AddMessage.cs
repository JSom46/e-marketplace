namespace Chats.Dtos;

public class AddMessage
{
    public Guid ChatId { get; set; }
    public string Content { get; set; }
    public IEnumerable<IFormFile> Attachments { get; set; }
}
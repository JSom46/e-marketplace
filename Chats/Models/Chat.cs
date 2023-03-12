namespace Chats.Models;

public class Chat
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }
    public string AuthorId { get; set; } = null!;
    public Announcement Announcement { get; set; } = null!;
    public ICollection<Message> Messages { get; } = new List<Message>();
}
namespace Chats.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public Guid AnnouncementId { get; set; }
        public string AuthorId { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}

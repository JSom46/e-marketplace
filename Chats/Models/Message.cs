namespace Chats.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string AuthorId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public bool Received { get; set; }
        public Guid ChatId { get; set; }
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }
}
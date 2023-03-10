namespace Chats.Models
{
    public class MessageAttachment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid MessageId { get; set; }
    }
}
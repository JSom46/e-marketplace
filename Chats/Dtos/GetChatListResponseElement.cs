namespace Chats.Dtos
{
    public class GetChatListResponseElement
    {
        public Guid Id { get; set; }
        public Guid AnnouncementId { get; set; }
        public string AuthorId { get; set; }
        public string LastMessage { get; set; }
    }
}

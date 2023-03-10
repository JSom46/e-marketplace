namespace Chats.Dtos
{
    public class GetChatResponse
    {
        public Guid AnnouncementId { get; set; }
        public string AuthorId { get; set; }
        public IEnumerable<GetMessageResponse> Messages { get; set; }
    }
}

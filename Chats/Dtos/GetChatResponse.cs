namespace Chats.Dtos;

public class GetChatResponse
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }
    public string AuthorId { get; set; }
    public IEnumerable<GetChatResponseMessage> Messages { get; set; }
}
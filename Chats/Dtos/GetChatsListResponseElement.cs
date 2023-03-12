namespace Chats.Dtos;

public class GetChatsListResponseElement
{
    public Guid Id { get; set; }
    public Guid AnnouncementId { get; set; }
    public string AuthorId { get; set; } = null!;
}

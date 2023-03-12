namespace Chats.Dtos;

public class GetChatsListResponse
{
    public IEnumerable<GetChatsListResponseElement> Chats { get; set; }
}

using System.Security.Claims;
using Chats.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chats.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ChatsDbContext _db;

    public ChatHub(ChatsDbContext db)
    {
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var chatIdStrings = Context.GetHttpContext().Request.Query["chatId"];

        if (chatIdStrings.Count != 1)
        {
            throw new HubException("ChatId not specified");
        }

        var userId = Context.GetHttpContext()?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            throw new HubException("Invalid bearer token");
        }

        Guid chatId;

        try
        {
            chatId = Guid.Parse(chatIdStrings[0]);
        }
        catch (Exception)
        {
            throw new HubException("Invalid Id");
        }

        var chat = await _db.Chats.FindAsync(chatId);

        if (chat == null)
        {
            throw new HubException("Chat not found");
        }

        var announcement = await _db.Announcements.FindAsync(chat.AnnouncementId);

        if (userId != chat.AuthorId && userId != announcement.AuthorId)
        {
            throw new HubException("Cannot access chat you're not part of");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatIdStrings[0]);

        await base.OnConnectedAsync();
    }
}
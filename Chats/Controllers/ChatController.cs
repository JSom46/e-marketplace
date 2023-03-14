using System.Security.Claims;
using Chats.DataAccess;
using Chats.Dtos;
using Chats.Hubs;
using Chats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;

namespace Chats.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatsDataAccess _chats;
    private readonly IHubContext<ChatHub, IChatClient> _hub;

    public ChatController(IChatsDataAccess chats, IHubContext<ChatHub, IChatClient> hub)
    {
        _chats = chats;
        _hub = hub;
    }

    // returns chat with specified id.
    [HttpGet]
    public async Task<ActionResult<GetChatResponse>> GetChat(Guid id)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get searched chat.
        var chat = await _chats.GetById(id, true, true);

        // chat not found.
        if (chat == null)
        {
            return NotFound();
        }

        // client does not participate in this chat.
        if (userId != chat.AuthorId && userId != chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

        // marked other user's messages as received.
        var receivedMessagesIds = new List<Guid>();

        foreach (var message in chat.Messages)
        {
            if (userId != message.AuthorId && !message.Received)
            {
                message.Received = true;
                receivedMessagesIds.Add(message.Id);
            }
        }

        // send information about received messages.
        if (receivedMessagesIds.Count > 0)
        {
            await _chats.UpdateMessages(chat.Messages);
            await _hub.Clients.Group(chat.Id.ToString()).MessagesDelivered(receivedMessagesIds);
        }

        return Ok(new GetChatResponse
        {
            Id = chat.Id,
            AuthorId = chat.AuthorId,
            AnnouncementId = chat.AnnouncementId,
            Messages = chat.Messages.Select(m => new GetChatResponseMessage
            {
                Id = m.Id,
                AuthorId = m.AuthorId,
                Content = m.Content,
                CreatedDate = m.CreatedDate,
                Received = m.Received,
                Attachments = m.MessageAttachments.Select(a => a.Name)
            })
        });
    }

    // returns list of chats a client participates in.
    [HttpGet]
    [Route("list")]
    public async Task<ActionResult<GetChatsListResponse>> GetChatsList()
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get searched chats.
        var chats = await _chats.GetByUserId(userId);

        return Ok(new GetChatsListResponse()
        {
            Chats = chats.Select(c => new GetChatsListResponseElement()
            {
                Id = c.Id,
                AuthorId = c.AuthorId,
                AnnouncementId = c.AnnouncementId
            })
        });
    }

    // returns message with specified id.
    [HttpGet]
    [Route("message")]
    public async Task<ActionResult<GetMessageResponse>> GetMessage(Guid id)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get searched message.
        var message = await _chats.GetMessageById(id);

        // message not found.
        if (message == null)
        {
            return NotFound();
        }

        // searched message belong to a chat the client does not participate in.
        if (userId != message.Chat.AuthorId && userId != message.Chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

        // if it's message from other user, mark it as received and send notification.
        if (userId != message.AuthorId && !message.Received)
        {
            message.Received = true;
            await _chats.UpdateMessages(new[] { message });
            await _hub.Clients.Group(message.Chat.Id.ToString()).MessagesDelivered(new[] { message.Id });
        }

        return Ok(new GetMessageResponse
        {
            Id = message.Id,
            AuthorId = message.AuthorId,
            CreatedDate = message.CreatedDate,
            Content = message.Content,
            Received = message.Received,
            Attachments = message.MessageAttachments.Select(a => a.Name)
        });
    }

    // returns file with specified name.
    [HttpGet]
    [Route("attachment")]
    public async Task<ActionResult> GetAttachment([FromQuery] string name)
    {
        try
        {
            // load file.
            var fileStream = await _chats.GetMessageAttachmentByName(name);

            // file not found.
            if (fileStream == null)
            {
                return NotFound();
            }

            // get content type of attachment.
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            contentType ??= "application/octet-stream";

            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }

    // create new chat.
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateChat([FromBody] AddChat chat)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get announcement associated with chat.
        var announcement = await _chats.GetAnnouncementById(chat.AnnouncementId);

        // announcement not found.
        if (announcement == null)
        {
            return BadRequest("Announcement with specified id does not exist");
        }

        // chat's author and announcement's author is same user.
        if (announcement.AuthorId == userId)
        {
            return BadRequest("Cannot create chat with yourself.");
        }

        // client already created chat associated with this announcement.
        if (await _chats.Exists(chat.AnnouncementId, userId))
        {
            return BadRequest("Cannot create multiple chats for the same announcement");
        }

        return await _chats.Add(new Chat
        {
            AnnouncementId = chat.AnnouncementId,
            AuthorId = userId
        });
    }

    // create new message.
    [HttpPost]
    [Route("message")]
    public async Task<ActionResult> AddMessage([FromForm] AddMessage message)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get chat associated with message.
        var chat = await _chats.GetById(message.ChatId, true);

        // chat not found.
        if (chat == null)
        {
            return NotFound("Chat with specified Id was not found");
        }

        // client does not participate in this chat.
        if (userId != chat.AuthorId && userId != chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

        // save message.
        var addedMessageId = await _chats.AddMessage(new Message
        {
            AuthorId = userId,
            ChatId = message.ChatId,
            Content = message.Content,
            CreatedDate = DateTimeOffset.Now,
            Received = false
        }, message.Attachments);

        // inform about new message
        await _hub.Clients.Group(chat.Id.ToString()).NewMessage(addedMessageId);

        return Ok(addedMessageId);
    }

    // deletes chat with specified id.
    [HttpDelete]
    public async Task<ActionResult> DeleteChat([FromBody] DeleteChat deleteChat)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // get chat.
        var chat = await _chats.GetById(deleteChat.ChatId);

        // chat not found
        if (chat == null)
        {
            return NotFound("Chat with specified Id was not found");
        }

        // client is not a chat's author.
        if (chat.AuthorId != userId)
        {
            return Unauthorized("Cannot delete someone else's chat");
        }

        // delete chat
        await _chats.DeleteChat(deleteChat.ChatId);

        return Ok();
    }
}
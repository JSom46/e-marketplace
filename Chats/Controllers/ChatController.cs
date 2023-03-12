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

    [HttpGet]
    public async Task<ActionResult<GetChatResponse>> GetChat(Guid id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        var chat = await _chats.GetById(id, true, true);

        if (chat == null)
        {
            return NotFound();
        }

        if (userId != chat.AuthorId && userId != chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

        var receivedMessagesIds = new List<Guid>();

        foreach (var message in chat.Messages)
        {
            if (userId != message.AuthorId && !message.Received)
            {
                message.Received = true;
                receivedMessagesIds.Add(message.Id);
            }
        }

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

    [HttpGet]
    [Route("list")]
    public async Task<ActionResult<GetChatsListResponse>> GetChatsList()
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        var chats = await _chats.GetByAuthorId(userId);

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

    [HttpGet]
    [Route("message")]
    public async Task<ActionResult<GetMessageResponse>> GetMessage(Guid id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        var message = await _chats.GetMessageById(id);

        if (message == null)
        {
            return NotFound();
        }

        if (userId != message.Chat.AuthorId && userId != message.Chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

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

    [HttpGet]
    [Route("attachment")]
    public async Task<ActionResult> GetAttachment([FromQuery] string name)
    {
        try
        {
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            contentType ??= "application/octet-stream";

            // Return the image file as a FileStreamResult
            var fileStream = await _chats.GetMessageAttachmentByName(name);

            if (fileStream == null)
            {
                return NotFound();
            }

            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateChat([FromBody] AddChat chat)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        var announcement = await _chats.GetAnnouncementById(chat.AnnouncementId);

        if (announcement == null)
        {
            return BadRequest("Announcement with specified id does not exist");
        }

        if (announcement.AuthorId == userId)
        {
            return BadRequest("Cannot create chat with yourself.");
        }

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

    [HttpPost]
    [Route("message")]
    public async Task<ActionResult> AddMessage([FromForm] AddMessage message)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        var chat = await _chats.GetById(message.ChatId, true);

        if (chat == null)
        {
            return NotFound("Chat with specified Id was not found");
        }

        if (userId != chat.AuthorId && userId != chat.Announcement.AuthorId)
        {
            return Unauthorized("Cannot access chat you're not part of");
        }

        var addedMessageId = await _chats.AddMessage(new Message
        {
            AuthorId = userId,
            ChatId = message.ChatId,
            Content = message.Content,
            CreatedDate = DateTimeOffset.Now,
            Received = false
        }, message.Attachments);

        await _hub.Clients.Group(chat.Id.ToString()).NewMessage(addedMessageId);

        return Ok(addedMessageId);
    }
}
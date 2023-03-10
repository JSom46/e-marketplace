using Chats.DataAccess;
using Chats.Dtos;
using Chats.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Chats.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FileManager;
using Microsoft.AspNetCore.StaticFiles;

namespace Chats.Controllers
{
    public class goowno
    {
        public Guid AnnouncementId { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatsContext _db;
        private readonly IHubContext<ChatHub, IChatClient> _hub;
        private readonly IFileManager _fileManager;

        public ChatController(ChatsContext db, IHubContext<ChatHub, IChatClient> hub, IFileManager fileManager)
        {
            _db = db;
            _hub = hub;
            _fileManager = fileManager;
        }

        [HttpGet]
        public async Task<ActionResult<GetChatResponse>> GetChat(Guid chatId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var chat = await _db.Chats.FindAsync(chatId);

            if (chat == null)
            {
                return NotFound("Chat with specified Id not found");
            }

            var announcement = await _db.Announcements.FindAsync(chat.AnnouncementId);

            if (userId != chat.AuthorId && userId != announcement.AuthorId)
            {
                return Unauthorized("Cannot access chat you're not part of");
            }

            var messages = await _db.Messages.Where(m => m.ChatId == chat.Id).OrderBy(m => m.CreatedDate).ToListAsync();

            var receivedMessagesIds = new List<Guid>();

            foreach (var message in messages)
            {
                if (userId != message.AuthorId && !message.Received)
                {
                    message.Received = true;
                    receivedMessagesIds.Add(message.Id);
                }
            }

            if (receivedMessagesIds.Count > 0)
            {
                await _db.SaveChangesAsync();
                await _hub.Clients.Group(chat.Id.ToString()).MessagesDelivered(receivedMessagesIds);
            }

            return Ok(new GetChatResponse()
            {
                AnnouncementId = announcement.Id,
                AuthorId = chat.AuthorId,
                Messages = messages.Select(m => new GetMessageResponse()
                {
                    Id = m.Id,
                    AuthorId = m.AuthorId,
                    Content = m.Content,
                    CreatedDate = m.CreatedDate,
                    Received = m.Received,
                    Attachments = m.Attachments.Select(a => a.Name)
                })
            });
        }

        [HttpGet]
        [Route("Message")]
        public async Task<ActionResult<GetMessageResponse>> GetMessage(Guid messageId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var message = await _db.Messages.FindAsync(messageId);

            if (message == null)
            {
                return NotFound("Message with specified Id not found");
            }

            var chat = await _db.Chats.FindAsync(message.ChatId);
            var announcement = await _db.Announcements.FindAsync(chat.AnnouncementId);

            if (userId != chat.AuthorId && userId != announcement.AuthorId)
            {
                return Unauthorized("Cannot access chat you're not part of");
            }

            if(userId != message.AuthorId && !message.Received)
            {
                message.Received = true;
                await _db.SaveChangesAsync();
                await _hub.Clients.Group(chat.Id.ToString()).MessagesDelivered(new[] { message.Id });
            }

            return Ok(new GetMessageResponse()
            {
                Id = message.Id,
                AuthorId = message.AuthorId,
                Content = message.Content,
                CreatedDate = message.CreatedDate,
                Received = message.Received,
                Attachments = message.Attachments.Select(a => a.Name)
            });
        }

        [HttpGet]
        [Route("Attachment")]
        public async Task<ActionResult> GetAttachment([FromQuery] string name)
        {
            try
            {
                new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
                contentType ??= "application/octet-stream";

                // Return the image file as a FileStreamResult
                var fileStream = await _fileManager.LoadFile(name);

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
        public async Task<ActionResult<Guid>> CreateChat([FromBody] goowno param)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var announcement = await _db.Announcements.FindAsync(param.AnnouncementId);

            if (announcement == null)
            {
                return BadRequest("Could not find announcement with specified Id");
            }

            if (announcement.AuthorId == userId)
            {
                return BadRequest("Cannot create chat with yourself.");
            }

            if (await _db.Chats.FirstOrDefaultAsync(c => c.AnnouncementId == param.AnnouncementId && c.AuthorId == userId) != null)
            {
                return BadRequest("Cannot create multiple chats for the same announcement");
            }

            var chat = _db.Chats.Add(new Chat()
            {
                AnnouncementId = param.AnnouncementId,
                AuthorId = userId
            }).Entity;
            await _db.SaveChangesAsync();

            return Ok(chat.Id);
        }

        [HttpPost]
        [Route("Message")]
        public async Task<ActionResult> AddMessage([FromForm] AddMessage addMessage)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var chat = await _db.Chats.FindAsync(addMessage.ChatId);

            if (chat == null)
            {
                return NotFound("Chat with specified Id was not found");
            }

            var announcement = await _db.Announcements.FindAsync(chat.AnnouncementId);

            if (!(announcement.AuthorId == userId || chat.AuthorId == userId))
            {
                return BadRequest("You do not belong to this chat");
            }

            var addedMessage = _db.Messages.Add(new Message()
            {
                AuthorId = userId,
                ChatId = chat.Id,
                Content = addMessage.Content,
                CreatedDate = DateTimeOffset.Now,
                Received = false,
            }).Entity;

            var processedFiles = new List<string>();
            var processedAttachments = new List<MessageAttachment>();

            try
            {
                foreach (var attachment in addMessage.Attachments)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
                    await _fileManager.SaveFile(attachment, fileName);
                    processedFiles.Add(fileName);

                    var processedAttachment = _db.MessageAttachments.Add(new MessageAttachment()
                    {
                        MessageId = addedMessage.Id,
                        Name = fileName
                    }).Entity;
                    processedAttachments.Add(processedAttachment);
                }
            }
            catch (Exception)
            {
                _db.MessageAttachments.RemoveRange(processedAttachments);

                var deleteFileTasks = new List<Task>();

                foreach (var processedFile in processedFiles)
                {
                    deleteFileTasks.Add(_fileManager.DeleteFile(processedFile));
                }

                await Task.WhenAll(deleteFileTasks);
                throw;
            }

            await _db.SaveChangesAsync();
            await _hub.Clients.Group(chat.Id.ToString()).NewMessage(addedMessage.Id);

            return Ok(addedMessage.Id);
        }
    }
}

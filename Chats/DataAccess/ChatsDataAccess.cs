using Chats.Models;
using FileManager;
using Microsoft.EntityFrameworkCore;

namespace Chats.DataAccess;

public class ChatsDataAccess : IChatsDataAccess
{
    private readonly ChatsDbContext _db;
    private readonly IFileManager _files;

    public ChatsDataAccess(ChatsDbContext db, IFileManager files)
    {
        _db = db;
        _files = files;
    }

    public async Task<Chat?> GetById(Guid id, bool includeAnnouncement = false, bool includeMessages = false)
    {
        if (includeAnnouncement && includeMessages)
        {
            return await _db.Chats
                .Include(c => c.Announcement)
                .Include(c => c.Messages)
                .ThenInclude(m => m.MessageAttachments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        if (includeAnnouncement)
        {
            return await _db.Chats
                .Include(c => c.Announcement)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        if (includeMessages)
        {
            return await _db.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.MessageAttachments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        return await _db.Chats.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Chat>> GetByUserId(string userId, bool includeMessages = false)
    {
        if (includeMessages)
        {
            return await _db.Chats
                .Include(c => c.Announcement)
                .Include(c => c.Messages)
                .ThenInclude(m => m.MessageAttachments)
                .Where(c => c.AuthorId == userId || c.Announcement.AuthorId == userId)
                .ToListAsync();
        }

        return await _db.Chats
            .Include(c => c.Announcement)
            .Where(c => c.AuthorId == userId || c.Announcement.AuthorId == userId)
            .ToListAsync();
    }

    public async Task<bool> Exists(Guid announcementId, string authorId)
    {
        return await _db.Chats.FirstOrDefaultAsync(c =>
            c.AnnouncementId == announcementId && c.AuthorId == authorId) != null;
    }

    public async Task<Guid> Add(Chat chat)
    {
        var addedChat = _db.Chats.Add(chat).Entity;
        await _db.SaveChangesAsync();
        return addedChat.Id;
    }

    public async Task<Announcement?> GetAnnouncementById(Guid id)
    {
        return await _db.Announcements.FindAsync(id);
    }

    public async Task<Message?> GetMessageById(Guid id)
    {
        return await _db.Messages
            .Include(m => m.MessageAttachments)
            .Include(m => m.Chat)
            .ThenInclude(c => c.Announcement)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Guid> AddMessage(Message message, IEnumerable<IFormFile> attachments)
    {
        var addedMessage = _db.Messages.Add(message).Entity;
        var addedAttachments = new List<MessageAttachment>();
        try
        {
            foreach (var attachment in attachments)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                addedAttachments.Add(_db.MessageAttachments.Add(new MessageAttachment
                {
                    MessageId = addedMessage.Id,
                    Name = fileName
                }).Entity);
                await _files.SaveFile(attachment, fileName);
            }

            await _db.SaveChangesAsync();
            return addedMessage.Id;
        }
        catch (Exception)
        {
            var deleteAttachmentTasks = new List<Task>();
            foreach (var attachment in addedAttachments)
            {
                deleteAttachmentTasks.Add(_files.DeleteFile(attachment.Name));
            }

            _db.RemoveRange(addedAttachments);
            await Task.WhenAll(deleteAttachmentTasks);
            throw;
        }
    }

    public async Task UpdateMessages(IEnumerable<Message> messages)
    {
        foreach (var message in messages)
        {
            _db.Messages.Update(message);
        }

        await _db.SaveChangesAsync();
    }

    public Task<FileStream?> GetMessageAttachmentByName(string name)
    {
        return _files.LoadFile(name);
    }

    public async Task DeleteChat(Guid id)
    {
        // get chat to be deleted together with associated messages and their attachments.
        var chat = await _db.Chats
            .Include(c => c.Messages)
            .ThenInclude(m => m.MessageAttachments)
            .FirstOrDefaultAsync(c => c.Id == id);

        // remove attached files.
        foreach (var message in chat.Messages)
        {
            foreach (var attachment in message.MessageAttachments)
            {
                await _files.DeleteFile(attachment.Name);
            }
        }

        // remove chat from database.
        _db.Chats.Remove(chat);

        // save changes.
        await _db.SaveChangesAsync();
    } 
}
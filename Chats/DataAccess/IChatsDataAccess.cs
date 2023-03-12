using Chats.Models;

namespace Chats.DataAccess;

public interface IChatsDataAccess
{
    Task<Chat?> GetById(Guid id, bool includeAnnouncement = false, bool includeMessages = false);
    Task<IEnumerable<Chat>> GetByAuthorId(string authorId, bool includeMessages = false);
    Task<Guid> Add(Chat chat);
    Task<bool> Exists(Guid announcementId, string authorId);
    Task<Announcement?> GetAnnouncementById(Guid id);
    Task<Message?> GetMessageById(Guid id);
    Task<Guid> AddMessage(Message message, IEnumerable<IFormFile> attachments);
    Task UpdateMessages(IEnumerable<Message> messages);
    Task<FileStream?> GetMessageAttachmentByName(string name);
}
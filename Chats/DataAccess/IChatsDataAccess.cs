using Chats.Models;

namespace Chats.DataAccess;

public interface IChatsDataAccess
{
    /// <summary>
    ///     Finds chat by its id.
    /// </summary>
    /// <param name="id">Id of chat to be returned.</param>
    /// <param name="includeAnnouncement">Should Announcement object the chat is related to be included.</param>
    /// <param name="includeMessages">Should Message objects related to the chat be included.</param>
    /// <returns>A <seealso cref="Chat" /> object containing data of searched chat or null, if such chat does not exist.</returns>
    Task<Chat?> GetById(Guid id, bool includeAnnouncement = false, bool includeMessages = false);

    /// <summary>
    ///     Finds chats related to specified user.
    /// </summary>
    /// <param name="userId">Id of user whose chats are to be returned.</param>
    /// <param name="includeMessages">Should Message objects related to the chat be included.</param>
    /// <returns>
    ///     An <seealso cref="IEnumerable{Chat}" /> of <seealso cref="Chat" /> objects that contains data of searched
    ///     chats.
    /// </returns>
    Task<IEnumerable<Chat>> GetByUserId(string userId, bool includeMessages = false);

    /// <summary>
    ///     Saves a new chat.
    /// </summary>
    /// <param name="chat">Data of chat to be saved.</param>
    /// <returns>A <seealso cref="Guid" /> assigned to saved chat.</returns>
    Task<Guid> Add(Chat chat);

    /// <summary>
    ///     Checks if user with specified Id already created a chat related to announcement with specified id.
    /// </summary>
    /// <param name="announcementId">Id of an announcement.</param>
    /// <param name="authorId">Id of a user.</param>
    /// <returns>true if such chat exists, false otherwise.</returns>
    Task<bool> Exists(Guid announcementId, string authorId);

    /// <summary>
    ///     Finds announcement by its id.
    /// </summary>
    /// <param name="id">Id of searched announcement.</param>
    /// <returns>
    ///     An <seealso cref="Announcement" /> object that contains data of searched announcement or null,
    ///     if such announcement does not exist.
    /// </returns>
    Task<Announcement?> GetAnnouncementById(Guid id);

    /// <summary>
    ///     Finds chat by its id.
    /// </summary>
    /// <param name="id">Id of searched message.</param>
    /// <returns>
    ///     A <seealso cref="Message" /> object containing data of searched message or null, if such message does not
    ///     exist.
    /// </returns>
    Task<Message?> GetMessageById(Guid id);

    /// <summary>
    ///     Saves a new message.
    /// </summary>
    /// <param name="message">Data of message to be saved.</param>
    /// <param name="attachments">Data of message's attachments to be saved.</param>
    /// <returns>A <seealso cref="Guid" /> assigned to saved message.</returns>
    Task<Guid> AddMessage(Message message, IEnumerable<IFormFile> attachments);

    /// <summary>
    ///     Updates already saved messages.
    /// </summary>
    /// <param name="messages">List of messages to be updated.</param>
    Task UpdateMessages(IEnumerable<Message> messages);

    /// <summary>
    ///     Finds attachment's file with specified name.
    /// </summary>
    /// <param name="name">Name of file to be returned.</param>
    /// <returns>A <seealso cref="FileStream" /> containing searched file or null if such file was not found.</returns>
    Task<FileStream?> GetMessageAttachmentByName(string name);
}
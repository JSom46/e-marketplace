using Announcements.Dtos;
using Announcements.Models;

namespace Announcements.DataAccess;

public interface IAnnouncementsDataAccess
{
    Task<Announcement?> GetById(Guid id);
    Task<IEnumerable<AnnouncementsListElement>> GetByAuthorId(string authorId);
    Task<IEnumerable<AnnouncementsListElement>> GetList(GetAnnouncementsList options);
    Task<int> GetPagesCount(GetAnnouncementsList options);
    Task<Guid> Add(Announcement announcement, List<IFormFile> pictures);
    Task<int> Update(Announcement announcement, List<IFormFile> pictures);
    Task<int> Delete(Guid id);
    Task<FileStream?> GetPicture(string fileName);
    Task<IEnumerable<string>> GetPicturesNames(Guid announcementId);
}
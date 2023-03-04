using Announcements.Dtos;
using Announcements.Models;

namespace Announcements.DataAccess;

public interface IAnnouncementsDataAccess
{
    Task<AnnouncementModel?> GetById(Guid id);
    Task<IEnumerable<AnnouncementsListElementModel>> GetByAuthorId(string authorId);
    Task<IEnumerable<AnnouncementsListElementModel>> GetList(ListAnnouncements options);
    Task<int> GetPagesCount(ListAnnouncements options);
    Task<Guid> Add(AnnouncementModel announcement, List<IFormFile> pictures);
    Task<int> Update(AnnouncementModel announcement, List<IFormFile> pictures);
    Task<int> Delete(Guid id);
    Task<FileStream?> GetPicture(string fileName);
    Task<IEnumerable<string>> GetPicturesNames(Guid announcementId);
}
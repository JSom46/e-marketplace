using Announcements.Dtos;
using Announcements.Models;

namespace Announcements.DataAccess;

public interface IAnnouncementsDataAccess
{
    Task<Announcement?> GetById(Guid id);
    Task<IEnumerable<Announcement>> GetByAuthorId(string authorId);
    Task<IEnumerable<Announcement>> GetList(ListAnnouncementsOptions options);
    Task<int> GetPagesCount(ListAnnouncementsOptions options);
    Task<Guid> Add(Announcement announcement);
    Task<int> Update(Announcement announcement);
    Task<int> Delete(Guid id);
}
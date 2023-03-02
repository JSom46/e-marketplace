using Announcements.Dtos;
using Announcements.Models;
using DataAccess;

namespace Announcements.DataAccess
{
    public class AnnouncementsDataAccess : IAnnouncementsDataAccess
    {
        private readonly IDataAccess _dataAccess;

        public AnnouncementsDataAccess(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<Announcement?> GetById(Guid id)
        {
            return await _dataAccess.QuerySingle<dynamic, Announcement>("SP_Announcements_getById", 
                new {Id = id});
        }

        public async Task<IEnumerable<Announcement>> GetByAuthorId(string authorId)
        {
            return await _dataAccess.Query<dynamic, Announcement>("SP_Announcements_getByAuthorId",
                new { AuthorId = authorId });
        }

        public async Task<IEnumerable<Announcement>> GetList(ListAnnouncementsOptions options)
        {
            return await _dataAccess.Query<ListAnnouncementsOptions, Announcement>("SP_Announcements_getPage", options);
        }

        public async Task<int> GetPagesCount(ListAnnouncementsOptions options)
        {
            return await _dataAccess.QuerySingle<dynamic, int>("SP_Announcements_getPagesCount", new
            {
                options.PageSize,
                options.Title,
                options.Category
            });
        }

        public async Task<Guid> Add(Announcement announcement)
        {
            return await _dataAccess.QuerySingle<dynamic, Guid>("SP_Announcements_add", new
            {
                announcement.AuthorId,
                announcement.Title,
                announcement.Description,
                announcement.Category,
                announcement.CreatedDate,
                announcement.ExpiresDate,
                announcement.IsActive,
                announcement.Price
            });
        }

        public async Task<int> Update(Announcement announcement)
        {
            return await _dataAccess.Execute<Announcement>("SP_Announcements_update", announcement);
        }

        public async Task<int> Delete(Guid id)
        {
            return await _dataAccess.Execute<dynamic>("SP_Announcements_delete", new { Id = id });
        }
    }
}

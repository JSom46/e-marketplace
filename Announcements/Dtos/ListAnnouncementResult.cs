using Announcements.Models;

namespace Announcements.Dtos
{
    public class ListAnnouncementResult
    {
        public int PagesCount { get; set; }
        public IEnumerable<Announcement> Announcements { get; set; }
    }
}

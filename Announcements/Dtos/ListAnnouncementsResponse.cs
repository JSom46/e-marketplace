using Announcements.Models;

namespace Announcements.Dtos
{
    public class ListAnnouncementResponseElement
    {
        public Guid Id { get; set; }
        public string AuthorId { get; set; }
        public string Title { get; set; }
        public int Category { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ExpiresDate { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        public string Picture { get; set; }
    }
    public class ListAnnouncementsResponse
    {
        public int PagesCount { get; set; }
        public IEnumerable<ListAnnouncementResponseElement> Announcements { get; set; }
    }
}

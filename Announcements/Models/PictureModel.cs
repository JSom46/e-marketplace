namespace Announcements.Models
{
    public class PictureModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Guid AnnouncementId { get; set; }
    }
}

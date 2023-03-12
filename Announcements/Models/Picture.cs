namespace Announcements.Models;

public class Picture
{
    public long Id { get; set; }
    public string Name { get; set; }
    public Guid AnnouncementId { get; set; }
}
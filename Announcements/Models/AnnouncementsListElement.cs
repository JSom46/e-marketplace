namespace Announcements.Models;

public class AnnouncementsListElement
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; }
    public string Title { get; set; }
    public int Category { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ExpiresDate { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string PictureName { get; set; }
}
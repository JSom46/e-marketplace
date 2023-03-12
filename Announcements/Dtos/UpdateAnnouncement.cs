namespace Announcements.Dtos;

public class UpdateAnnouncement
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
    public List<IFormFile> Pictures { get; set; }
}
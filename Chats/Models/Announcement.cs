namespace Chats.Models;

public class Announcement
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Category { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ExpiresDate { get; set; }
    public bool IsActive { get; set; }
    public decimal Price { get; set; }
    public virtual ICollection<Chat> Chats { get; } = new List<Chat>();
}
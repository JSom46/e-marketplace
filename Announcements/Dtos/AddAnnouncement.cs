namespace Announcements.Dtos
{
    public class AddAnnouncement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }
        public Decimal Price { get; set; }
    }
}

namespace Announcements.Dtos
{
    public class ListAnnouncements
    {
        public int? PageNumber { set; get; }
        public int? PageSize { set; get; }
        public string? Title { set; get; }
        public string? SortColumn { set; get; }
        public bool? Ascending { set; get; }
        public int? Category { set; get; }
    }
}

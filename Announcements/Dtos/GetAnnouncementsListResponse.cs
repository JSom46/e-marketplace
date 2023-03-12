namespace Announcements.Dtos;

public class GetAnnouncementsListResponse
{
    public int PagesCount { get; set; }
    public IEnumerable<GetAnnouncementsListResponseElement> Announcements { get; set; }
}
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Claims;
using Announcements.DataAccess;
using Announcements.Dtos;
using Announcements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Announcements.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementsDataAccess _announcements;

        public AnnouncementController(IAnnouncementsDataAccess announcements)
        {
            _announcements = announcements;
        }

        [HttpGet]
        public async Task<ActionResult<Announcement>> GetAnnouncementById([FromQuery] Guid id)
        {
            Console.WriteLine(id);

            /*if (Guid.TryParse(id, out var parsedId))
            {
                return BadRequest("Invalid id");
            }
            Console.WriteLine(parsedId);*/
            var res = await _announcements.GetById(id);

            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }

        [HttpGet]
        [Authorize]
        [Route("my")]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetUsersAnnouncements()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            return Ok(await _announcements.GetByAuthorId(userId));
        }

        [HttpGet]
        [Route("list")]
        public async Task<ActionResult<ListAnnouncementResult>> GetAnnouncements([FromQuery] ListAnnouncementsOptions options)
        {
            var pagesCount = await _announcements.GetPagesCount(options);

            if (options.PageNumber > pagesCount)
            {
                return Ok(new ListAnnouncementResult()
                {
                    PagesCount = pagesCount,
                    Announcements = new List<Announcement>()
                });
            }

            var announcements = await _announcements.GetList(options);

            return Ok(new ListAnnouncementResult()
            {
                PagesCount = pagesCount,
                Announcements = announcements
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Guid>> AddAnnouncement([FromBody] AddAnnouncement addAnnouncement)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            if (addAnnouncement.Price < 0)
            {
                return BadRequest("Price cannot be negative");
            }

            if (addAnnouncement.Title.Length < 3)
            {
                return BadRequest("Title cannot be shorter than 3 characters");
            }

            var announcementId = await _announcements.Add(new Announcement()
            {
                Title = addAnnouncement.Title,
                Description = addAnnouncement.Description,
                Category = addAnnouncement.Category,
                Price = addAnnouncement.Price,
                AuthorId = userId,
                CreatedDate = DateTimeOffset.Now,
                ExpiresDate = DateTimeOffset.Now + TimeSpan.FromDays(30),
                IsActive = true
            });

            return Ok(announcementId);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> UpdateAnnouncement([FromBody] UpdateAnnouncement updateAnnouncement)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            if (updateAnnouncement.Price < 0)
            {
                return BadRequest("Price cannot be negative");
            }

            if (updateAnnouncement.Title.Length < 3)
            {
                return BadRequest("Title cannot be shorter than 3 characters");
            }

            var announcement = await _announcements.GetById(updateAnnouncement.Id);

            if (announcement == null)
            {
                return NotFound();
            }

            if (announcement.AuthorId != userId)
            {
                return Unauthorized("Users Id doesn't match author's Id");
            }

            await _announcements.Update(new Announcement()
            {
                Id = updateAnnouncement.Id,
                Title = updateAnnouncement.Title,
                Description = updateAnnouncement.Description,
                Category = updateAnnouncement.Category,
                Price = updateAnnouncement.Price,
                AuthorId = announcement.AuthorId,
                CreatedDate = announcement.CreatedDate,
                ExpiresDate = announcement.ExpiresDate,
                IsActive = announcement.IsActive
            });

            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public async Task<ActionResult> DeleteAnnouncement([FromBody] Guid id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var announcement = await _announcements.GetById(id);

            if (announcement == null)
            {
                return NotFound();
            }

            if (announcement.AuthorId != userId)
            {
                return Unauthorized("Users Id doesn't match author's Id");
            }

            await _announcements.Delete(id);

            return Ok();
        }
    }
}

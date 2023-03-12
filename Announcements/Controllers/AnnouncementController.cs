using System.Security.Claims;
using Announcements.DataAccess;
using Announcements.Dtos;
using Announcements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Announcements.Controllers;

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
    public async Task<ActionResult<GetAnnouncementResponse>> GetAnnouncementById([FromQuery] Guid id)
    {
        try
        {
            var announcement = await _announcements.GetById(id);

            if (announcement == null)
            {
                return NotFound();
            }

            var pictures = await _announcements.GetPicturesNames(id);

            return Ok(new GetAnnouncementResponse
            {
                Id = announcement.Id,
                AuthorId = announcement.AuthorId,
                Title = announcement.Title,
                Description = announcement.Description,
                Category = announcement.Category,
                CreatedDate = announcement.CreatedDate,
                ExpiresDate = announcement.ExpiresDate,
                IsActive = announcement.IsActive,
                Price = announcement.Price,
                Pictures = pictures
            });
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    [HttpGet]
    [Authorize]
    [Route("my")]
    public async Task<ActionResult<GetAnnouncementsListResponse>> GetUsersAnnouncements()
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            var announcements = await _announcements.GetByAuthorId(userId);

            return Ok(new GetAnnouncementsListResponse
            {
                PagesCount = 1,
                Announcements = announcements.Select(e => new GetAnnouncementsListResponseElement
                {
                    Id = e.Id,
                    AuthorId = e.AuthorId,
                    Title = e.Title,
                    Category = e.Category,
                    CreatedDate = e.CreatedDate,
                    ExpiresDate = e.ExpiresDate,
                    IsActive = e.IsActive,
                    Price = e.Price,
                    Picture = e.PictureName
                })
            });
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    [HttpGet]
    [Route("list")]
    public async Task<ActionResult<GetAnnouncementsListResponse>> GetAnnouncements(
        [FromQuery] GetAnnouncementsList options)
    {
        try
        {
            var pagesCount = await _announcements.GetPagesCount(options);

            if (options.PageNumber > pagesCount)
            {
                return Ok(new GetAnnouncementsListResponse
                {
                    PagesCount = pagesCount,
                    Announcements = new List<GetAnnouncementsListResponseElement>()
                });
            }

            var announcements = await _announcements.GetList(options);

            return Ok(new GetAnnouncementsListResponse
            {
                PagesCount = pagesCount,
                Announcements = announcements.Select(e => new GetAnnouncementsListResponseElement
                {
                    Id = e.Id,
                    AuthorId = e.AuthorId,
                    Title = e.Title,
                    Category = e.Category,
                    CreatedDate = e.CreatedDate,
                    ExpiresDate = e.ExpiresDate,
                    IsActive = e.IsActive,
                    Price = e.Price,
                    Picture = e.PictureName
                })
            });
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> AddAnnouncement([FromForm] AddAnnouncement addAnnouncement)
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

        if (addAnnouncement.Pictures.Count > 8)
        {
            return BadRequest("Cannot upload more than 8 pictures");
        }

        foreach (var picture in addAnnouncement.Pictures)
        {
            if (picture.ContentType.ToLower() != "image/jpeg" &&
                picture.ContentType.ToLower() != "image/png" &&
                picture.ContentType.ToLower() != "image/gif")
            {
                return BadRequest("Cannot upload files that are not images");
            }

            if (picture.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Cannot upload files bigger than 5MB");
            }
        }

        try
        {
            var announcementId = await _announcements.Add(new Announcement
            {
                Title = addAnnouncement.Title,
                Description = addAnnouncement.Description,
                Category = addAnnouncement.Category,
                Price = addAnnouncement.Price,
                AuthorId = userId,
                CreatedDate = DateTimeOffset.Now,
                ExpiresDate = DateTimeOffset.Now + TimeSpan.FromDays(30),
                IsActive = true
            }, addAnnouncement.Pictures);

            return Ok(announcementId);
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult> UpdateAnnouncement([FromForm] UpdateAnnouncement updateAnnouncement)
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

        if (updateAnnouncement.Pictures.Count > 8)
        {
            return BadRequest("Cannot upload more than 8 pictures");
        }

        foreach (var picture in updateAnnouncement.Pictures)
        {
            if (picture.ContentType.ToLower() != "image/jpeg" &&
                picture.ContentType.ToLower() != "image/png" &&
                picture.ContentType.ToLower() != "image/gif")
            {
                return BadRequest("Cannot upload files that are not images");
            }

            if (picture.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Cannot upload files bigger than 5MB");
            }
        }

        try
        {
            var announcement = await _announcements.GetById(updateAnnouncement.Id);

            if (announcement == null)
            {
                return NotFound();
            }

            if (announcement.AuthorId != userId)
            {
                return Unauthorized("Users Id doesn't match author's Id");
            }

            await _announcements.Update(new Announcement
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
            }, updateAnnouncement.Pictures);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
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

        try
        {
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
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    [HttpGet]
    [Route("picture")]
    public async Task<ActionResult> GetPicture([FromQuery] string name)
    {
        try
        {
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            contentType ??= "application/octet-stream";

            // Return the image file as a FileStreamResult
            var fileStream = await _announcements.GetPicture(name);

            if (fileStream == null)
            {
                return NotFound();
            }

            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }
}
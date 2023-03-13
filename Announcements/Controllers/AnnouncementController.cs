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

    // returns announcement with specified id.
    [HttpGet]
    public async Task<ActionResult<GetAnnouncementResponse>> GetAnnouncementById([FromQuery] Guid id)
    {
        try
        {
            // get searched announcement.
            var announcement = await _announcements.GetById(id);

            // announcement not found.
            if (announcement == null)
            {
                return NotFound();
            }

            // get names of pictures associated with announcement.
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

    // returns announcements added by client.
    [HttpGet]
    [Authorize]
    [Route("my")]
    public async Task<ActionResult<GetAnnouncementsListResponse>> GetUsersAnnouncements()
    {
        try
        {
            // get client's id from bearer token.
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // id not found.
            if (userId == null)
            {
                return Unauthorized("Invalid bearer token");
            }

            // get client's announcements.
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

    // returns list of announcements divided into pages.
    [HttpGet]
    [Route("list")]
    public async Task<ActionResult<GetAnnouncementsListResponse>> GetAnnouncements(
        [FromQuery] GetAnnouncementsList options)
    {
        try
        {
            // get total number of pages.
            var pagesCount = await _announcements.GetPagesCount(options);

            // total number of pages is greater than number of requested page - empty list is returned.
            if (options.PageNumber > pagesCount)
            {
                return Ok(new GetAnnouncementsListResponse
                {
                    PagesCount = pagesCount,
                    Announcements = new List<GetAnnouncementsListResponseElement>()
                });
            }
            
            // get requested page.
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

    // creates new announcement.
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> AddAnnouncement([FromForm] AddAnnouncement addAnnouncement)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // price is negative.
        if (addAnnouncement.Price < 0)
        {
            return BadRequest("Price cannot be negative");
        }

        // title is too short.
        if (addAnnouncement.Title.Length < 3)
        {
            return BadRequest("Title cannot be shorter than 3 characters");
        }

        // too many pictures attached.
        if (addAnnouncement.Pictures.Count > 8)
        {
            return BadRequest("Cannot upload more than 8 pictures");
        }

        foreach (var picture in addAnnouncement.Pictures)
        {
            // attached file is not a picture.
            if (picture.ContentType.ToLower() != "image/jpeg" &&
                picture.ContentType.ToLower() != "image/png" &&
                picture.ContentType.ToLower() != "image/gif")
            {
                return BadRequest("Cannot upload files that are not images");
            }

            // picture's size is greater than 5MB.
            if (picture.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Cannot upload files bigger than 5MB");
            }
        }

        try
        {
            // save announcement.
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

    // Updates existing announcement previously created by client.
    [HttpPut]
    [Authorize]
    public async Task<ActionResult> UpdateAnnouncement([FromForm] UpdateAnnouncement updateAnnouncement)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        // price is negative.
        if (updateAnnouncement.Price < 0)
        {
            return BadRequest("Price cannot be negative");
        }

        // title is too short.
        if (updateAnnouncement.Title.Length < 3)
        {
            return BadRequest("Title cannot be shorter than 3 characters");
        }

        // title is too short.
        if (updateAnnouncement.Pictures.Count > 8)
        {
            return BadRequest("Cannot upload more than 8 pictures");
        }

        foreach (var picture in updateAnnouncement.Pictures)
        {
            // attached file is not a picture.
            if (picture.ContentType.ToLower() != "image/jpeg" &&
                picture.ContentType.ToLower() != "image/png" &&
                picture.ContentType.ToLower() != "image/gif")
            {
                return BadRequest("Cannot upload files that are not images");
            }

            // picture's size is greater than 5MB.
            if (picture.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Cannot upload files bigger than 5MB");
            }
        }

        try
        {
            // get announcement to be updated.
            var announcement = await _announcements.GetById(updateAnnouncement.Id);

            // announcement not found.
            if (announcement == null)
            {
                return NotFound();
            }

            // client is not the author of this announcement.
            if (announcement.AuthorId != userId)
            {
                return Unauthorized("Users Id doesn't match author's Id");
            }

            // update announcement.
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

    // deletes announcement previously created by client.
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteAnnouncement([FromBody] Guid id)
    {
        // get client's id from bearer token.
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // id not found.
        if (userId == null)
        {
            return Unauthorized("Invalid bearer token");
        }

        try
        {
            // get announcement to be deleted.
            var announcement = await _announcements.GetById(id);

            // announcement not found.
            if (announcement == null)
            {
                return NotFound();
            }

            // client is not the author of this announcement.
            if (announcement.AuthorId != userId)
            {
                return Unauthorized("Users Id doesn't match author's Id");
            }

            // delete announcement.
            await _announcements.Delete(id);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            return StatusCode(500);
        }
    }

    // returns picture with specified name.
    [HttpGet]
    [Route("picture")]
    public async Task<ActionResult> GetPicture([FromQuery] string name)
    {
        try
        {
            // load picture.
            var fileStream = await _announcements.GetPicture(name);

            // picture not found
            if (fileStream == null)
            {
                return NotFound();
            }

            // get content type.
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            contentType ??= "application/octet-stream";

            return File(fileStream, contentType);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500);
        }
    }
}
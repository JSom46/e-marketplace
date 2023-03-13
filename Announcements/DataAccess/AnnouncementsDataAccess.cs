using Announcements.Dtos;
using Announcements.Models;
using DataAccess;
using FileManager;

namespace Announcements.DataAccess;

public class AnnouncementsDataAccess : IAnnouncementsDataAccess
{
    private readonly IDataAccess _dataAccess;
    private readonly IFileManager _fileManager;

    public AnnouncementsDataAccess(IDataAccess dataAccess, IFileManager fileManager)
    {
        _dataAccess = dataAccess;
        _fileManager = fileManager;
    }

    public async Task<Announcement?> GetById(Guid id)
    {
        return await _dataAccess.QuerySingle<dynamic, Announcement>("SP_Announcements_getById",
            new { Id = id });
    }

    public async Task<IEnumerable<AnnouncementsListElement>> GetByAuthorId(string authorId)
    {
        return await _dataAccess.Query<dynamic, AnnouncementsListElement>("SP_Announcements_getByAuthorId",
            new { AuthorId = authorId });
    }

    public async Task<IEnumerable<AnnouncementsListElement>> GetList(GetAnnouncementsList options)
    {
        return await _dataAccess.Query<GetAnnouncementsList, AnnouncementsListElement>("SP_Announcements_getPage",
            options);
    }

    public async Task<int> GetPagesCount(GetAnnouncementsList options)
    {
        return await _dataAccess.QuerySingle<dynamic, int>("SP_Announcements_getPagesCount", new
        {
            options.PageSize,
            options.Title,
            options.Category
        });
    }

    public async Task<Guid> Add(Announcement announcement, List<IFormFile> pictures)
    {
        _dataAccess.StartTransaction();
        List<string> processedPictures = new();

        try
        {
            var announcementId = await _dataAccess.QuerySingleInTransaction<dynamic, Guid>("SP_Announcements_add",
                new
                {
                    announcement.AuthorId,
                    announcement.Title,
                    announcement.Description,
                    announcement.Category,
                    announcement.CreatedDate,
                    announcement.ExpiresDate,
                    announcement.IsActive,
                    announcement.Price
                });

            foreach (var picture in pictures)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(picture.FileName);
                processedPictures.Add(fileName);
                var persistFileTask = _fileManager.SaveFile(picture, fileName);
                var persistDataTask = _dataAccess.ExecuteInTransaction<dynamic>("SP_Pictures_add",
                    new
                    {
                        AnnouncementId = announcementId,
                        Name = fileName
                    });
                await Task.WhenAll(persistFileTask, persistDataTask);
            }

            _dataAccess.CommitTransaction();

            return announcementId;
        }
        catch (Exception)
        {
            List<Task> deletePictureTasks = new();

            foreach (var processedPicture in processedPictures)
            {
                deletePictureTasks.Add(_fileManager.DeleteFile(processedPicture));
            }

            await Task.WhenAll(deletePictureTasks);
            _dataAccess.RollbackTransaction();
            throw;
        }
    }

    public async Task<int> Update(Announcement announcement, List<IFormFile> pictures)
    {
        List<string> processedPictures = new();
        var oldPictures = await _dataAccess.Query<dynamic, Picture>(
            "SP_Pictures_getByAnnouncementId",
            new
            {
                AnnouncementId = announcement.Id
            });

        _dataAccess.StartTransaction();

        try
        {
            var res = await _dataAccess.ExecuteInTransaction("SP_Announcements_update", announcement);
            await _dataAccess.ExecuteInTransaction<dynamic>("SP_Pictures_deleteByAnnouncementId",
                new
                {
                    AnnouncementId = announcement.Id
                });

            foreach (var picture in pictures)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(picture.FileName);
                processedPictures.Add(fileName);
                var persistFileTask = _fileManager.SaveFile(picture, fileName);
                var persistDataTask = _dataAccess.ExecuteInTransaction<dynamic>("SP_Pictures_add",
                    new
                    {
                        AnnouncementId = announcement.Id,
                        Name = fileName
                    });
                await Task.WhenAll(persistFileTask, persistDataTask);
            }

            List<Task> deleteFileTasks = new();

            foreach (var oldPicture in oldPictures)
            {
                Console.WriteLine(oldPicture);
                deleteFileTasks.Add(_fileManager.DeleteFile(oldPicture.Name));
            }

            await Task.WhenAll(deleteFileTasks);

            _dataAccess.CommitTransaction();

            return res;
        }
        catch (Exception)
        {
            List<Task> deletePictureTasks = new();

            foreach (var processedPicture in processedPictures)
            {
                deletePictureTasks.Add(_fileManager.DeleteFile(processedPicture));
            }

            await Task.WhenAll(deletePictureTasks);
            _dataAccess.RollbackTransaction();
            throw;
        }
    }

    public async Task<int> Delete(Guid id)
    {
        _dataAccess.StartTransaction();
        try
        {
            var pictures = await _dataAccess.QueryInTransaction<dynamic, Picture>(
                "SP_Pictures_getByAnnouncementId",
                new
                {
                    AnnouncementId = id
                });

            await _dataAccess.ExecuteInTransaction<dynamic>("SP_Pictures_deleteByAnnouncementId",
                new
                {
                    AnnouncementId = id
                });

            var res = await _dataAccess.ExecuteInTransaction<dynamic>("SP_Announcements_delete",
                new
                {
                    Id = id
                });

            List<Task> deleteFileTasks = new();

            foreach (var picture in pictures)
            {
                deleteFileTasks.Add(_fileManager.DeleteFile(picture.Name));
            }

            await Task.WhenAll(deleteFileTasks);

            return res;
        }
        catch (Exception)
        {
            _dataAccess.RollbackTransaction();
            throw;
        }
    }

    public Task<FileStream?> GetPicture(string fileName)
    {
        return _fileManager.LoadFile(fileName);
    }

    public async Task<IEnumerable<string>> GetPicturesNames(Guid announcementId)
    {
        return await _dataAccess.Query<dynamic, string>("SP_Pictures_getByAnnouncementId",
            new
            {
                AnnouncementId = announcementId
            });
    }
}
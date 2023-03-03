using Announcements.Dtos;
using Announcements.Models;
using DataAccess;
using FileManager;

namespace Announcements.DataAccess
{
    public class AnnouncementsDataAccess : IAnnouncementsDataAccess
    {
        private readonly IDataAccess _dataAccess;
        private readonly IFileManager _fileManager;

        public AnnouncementsDataAccess(IDataAccess dataAccess, IFileManager fileManager)
        {
            _dataAccess = dataAccess;
            _fileManager = fileManager;
        }

        public async Task<AnnouncementModel?> GetById(Guid id)
        {
            return await _dataAccess.QuerySingle<dynamic, AnnouncementModel>("SP_Announcements_getById", 
                new {Id = id});
        }

        public async Task<IEnumerable<AnnouncementsListElementModel>> GetByAuthorId(string authorId)
        {
            return await _dataAccess.Query<dynamic, AnnouncementsListElementModel>("SP_Announcements_getByAuthorId",
                new { AuthorId = authorId });
        }

        public async Task<IEnumerable<AnnouncementsListElementModel>> GetList(ListAnnouncements options)
        {
            return await _dataAccess.Query<ListAnnouncements, AnnouncementsListElementModel>("SP_Announcements_getPage", options);
        }

        public async Task<int> GetPagesCount(ListAnnouncements options)
        {
            return await _dataAccess.QuerySingle<dynamic, int>("SP_Announcements_getPagesCount", new
            {
                options.PageSize,
                options.Title,
                options.Category
            });
        }

        public async Task<Guid> Add(AnnouncementModel announcement, List<IFormFile> pictures)
        {
            _dataAccess.StartTransaction();
            var processedPictures = new List<string>();

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
                var deletePictureTasks = new List<Task>();

                foreach (var processedPicture in processedPictures)
                {
                    deletePictureTasks.Add(_fileManager.DeleteFile(processedPicture));
                }

                await Task.WhenAll(deletePictureTasks);
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public async Task<int> Update(AnnouncementModel announcement, List<IFormFile> pictures)
        {
            var processedPictures = new List<string>();
            var oldPictures = await _dataAccess.Query<dynamic, PictureModel>("SP_Pictures_getByAnnouncementId",
                new
                {
                    AnnouncementId = announcement.Id
                });

            _dataAccess.StartTransaction();

            try
            {
                var res = await _dataAccess.ExecuteInTransaction<AnnouncementModel>("SP_Announcements_update", announcement);
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

                var deleteFileTasks = new List<Task>();

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
                var deletePictureTasks = new List<Task>();

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
                var pictures = await _dataAccess.QueryInTransaction<dynamic, PictureModel>("SP_Pictures_getByAnnouncementId",
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

                var deleteFileTasks = new List<Task>();

                foreach (var picture in pictures)
                {
                    deleteFileTasks.Add(_fileManager.DeleteFile(picture.Name));
                }

                await Task.WhenAll(deleteFileTasks);

                return res;
            }
            catch(Exception)
            {
                _dataAccess.RollbackTransaction();
                throw;
            }
        }

        public Task<FileStream> GetPicture(string fileName)
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
}

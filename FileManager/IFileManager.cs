using Microsoft.AspNetCore.Http;

namespace FileManager;

public interface IFileManager
{
    Task SaveFile(IFormFile file, string? filename);
    Task<FileStream?> LoadFile(string fileName);
    Task DeleteFile(string fileName);
}
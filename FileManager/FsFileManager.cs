using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FileManager;

public class FsFileManager : IFileManager
{
    private readonly IConfiguration _config;
    private readonly string _path;

    public FsFileManager(IConfiguration config)
    {
        _config = config;
        _path = _config.GetSection("DefaultFilePath")?.Value ?? "/";
    }

    public async Task SaveFile(IFormFile file, string? filename)
    {
        await using var stream = new FileStream(Path.Combine(_path, filename ?? file.FileName), FileMode.CreateNew);
        await file.CopyToAsync(stream);
    }

    public Task<FileStream?> LoadFile(string fileName)
    {
        try
        {
            return Task.FromResult(new FileStream(Path.Combine(_path, fileName), FileMode.Open, FileAccess.Read));
        }
        catch (FileNotFoundException)
        {
            return Task.FromResult<FileStream?>(null);
        }
    }

    public Task DeleteFile(string fileName)
    {
        File.Delete(Path.Combine(_path, fileName));
        return Task.CompletedTask;
    }
}
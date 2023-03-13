using Microsoft.AspNetCore.Http;

namespace FileManager;

public interface IFileManager
{
    /// <summary>
    /// Saves file
    /// </summary>
    /// <param name="file">File to be saved.</param>
    /// <param name="filename">Name under which the file should be saved. If not provided, file's original name is used.</param>
    Task SaveFile(IFormFile file, string? filename);

    /// <summary>
    /// Loads file with specified name.
    /// </summary>
    /// <param name="fileName">Name of a file to be loaded</param>
    /// <returns>Loaded file or null, if file with specified name was not found.</returns>
    Task<FileStream?> LoadFile(string fileName);

    /// <summary>
    /// Deletes file with specified name.
    /// </summary>
    /// <param name="fileName"></param>
    Task DeleteFile(string fileName);
}
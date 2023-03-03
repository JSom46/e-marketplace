﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FileManager
{
    public class FsFileManager : IFileManager
    {
        private readonly string _path;
        private readonly IConfiguration _config;

        public FsFileManager(IConfiguration config)
        {
            _config = config;
            Console.WriteLine("filemenedzer " + _config.GetSection("DefaultFilePath").Value);
            //_path = Environment.GetEnvironmentVariable("DEFAULT_FILE_PATH") ?? "/";
            _path = _config.GetSection("DefaultFilePath")?.Value ?? "/";
        }

        public async Task SaveFile(IFormFile file, string? filename)
        {
            using (var stream = new FileStream(Path.Combine(_path, filename ?? file.FileName), FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }
        }

        public Task<FileStream> LoadFile(string fileName)
        {
            return Task.FromResult(new FileStream(Path.Combine(_path, fileName), FileMode.Open, FileAccess.Read));
        }

        public Task DeleteFile(string fileName)
        {
            File.Delete(Path.Combine(_path, fileName));
            return Task.CompletedTask;
        }
    }
}
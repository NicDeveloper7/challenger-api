using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Challenger.App.Services
{
    public interface IStorageService
    {
        Task<string> SaveAsync(Stream content, string contentType, string folder, string fileName);
    }

    public class LocalStorageService : IStorageService
    {
        private readonly string _basePath;

        public LocalStorageService(IConfiguration configuration)
        {
            var configured = configuration["Storage:Local:BasePath"];
            _basePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "storage")
                : configured;
        }

        public async Task<string> SaveAsync(Stream content, string contentType, string folder, string fileName)
        {
            Directory.CreateDirectory(_basePath);
            var targetFolder = Path.Combine(_basePath, folder);
            Directory.CreateDirectory(targetFolder);

            var ext = Path.GetExtension(fileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(targetFolder, name);

            using var output = new FileStream(fullPath, FileMode.Create);
            await content.CopyToAsync(output);

            // Return relative path from base path for persistence
            var relativePath = Path.Combine(folder, name).Replace("\\", "/");
            return relativePath;
        }
    }
}

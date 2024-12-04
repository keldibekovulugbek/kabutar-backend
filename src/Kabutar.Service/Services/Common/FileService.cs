using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Kabutar.Service.Helpers;
using Kabutar.Service.Interfaces.Common;

namespace Kabutar.Service.Services.Common
{
    public class FileService : IFileService
    {
        private readonly string _basePath = string.Empty;
        private readonly string _imageFolderName = "images";

        private readonly string _profileImagesFolderName = "profile-images";

        public FileService(IWebHostEnvironment webHost)
        {
            _basePath = webHost.WebRootPath;
        }

        string IFileService.ImageFolderName => _imageFolderName;

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image is null)
                return "";

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            if (!Directory.Exists(Path.Combine(_basePath, _imageFolderName,_profileImagesFolderName)))
            {
                Directory.CreateDirectory(Path.Combine(_basePath, _imageFolderName, _profileImagesFolderName));
            }

            string fileName = ImageHelper.MakeImageName(image.FileName);
            string partPath = Path.Combine(_imageFolderName,_profileImagesFolderName, fileName);
            string path = Path.Combine(_basePath, partPath);

            var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Close();

            return partPath;
        }

        public Task<bool> DeleteImageAsync(string relativeFilePath)
        {
            string absoluteFilePath = Path.Combine(_basePath, relativeFilePath);

            if (!File.Exists(absoluteFilePath)) return Task.FromResult(false);

            try
            {
                File.Delete(absoluteFilePath);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}

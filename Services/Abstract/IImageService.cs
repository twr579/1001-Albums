using _1001Albums.Models;

namespace _1001Albums.Services.Abstract
{
    public interface IImageService
    {
        bool IsImage(IFormFile file);
        Task<string> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig);
        Task DeleteFileFromStorage(string fileName, AzureStorageConfig _storageConfig);

    }
}

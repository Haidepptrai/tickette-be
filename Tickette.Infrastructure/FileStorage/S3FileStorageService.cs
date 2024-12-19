using Tickette.Application.Common.Interfaces;

namespace Tickette.Infrastructure.FileStorage;

public class S3FileStorageService : IFileStorageService
{
    public async Task<string> UploadFileAsync(IFileUpload file, string folder)
    {
        await Task.CompletedTask; // Simulate async behavior
        return "https://picsum.photos/200";
    }
}
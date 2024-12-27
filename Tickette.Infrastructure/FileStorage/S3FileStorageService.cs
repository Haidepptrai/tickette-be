using Tickette.Application.Common.Interfaces;

namespace Tickette.Infrastructure.FileStorage;

public class S3FileStorageService : IFileStorageService
{
    public async Task<string?> UploadFileAsync(IFileUpload? file, string folder)
    {
        if (file == null)
        {
            return null;
        }
        await Task.CompletedTask; // Simulate async behavior
        return "https://picsum.photos/200";
    }
}
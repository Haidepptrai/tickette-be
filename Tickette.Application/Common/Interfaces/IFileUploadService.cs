namespace Tickette.Application.Common.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(IFileUpload file, string folder);
    Task<string> UploadModelAsync(string filePath, string folder);
}
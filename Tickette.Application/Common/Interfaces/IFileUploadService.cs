namespace Tickette.Application.Common.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFileUpload file, string folder);
}
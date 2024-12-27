namespace Tickette.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string?> UploadFileAsync(IFileUpload? file, string folder);
}
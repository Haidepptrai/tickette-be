namespace Tickette.Application.Common.Interfaces;

public interface IFileUpload
{
    string FileName { get; }
    string ContentType { get; }
    Task<Stream> OpenReadStreamAsync();
}
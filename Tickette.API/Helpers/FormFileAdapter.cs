using Tickette.Application.Common.Interfaces;

namespace Tickette.API.Helpers;

public class FormFileAdapter : IFileUpload
{
    private readonly IFormFile _formFile;

    public FormFileAdapter(IFormFile formFile)
    {
        _formFile = formFile;
    }

    public string FileName => _formFile.FileName;

    public string ContentType => _formFile.ContentType;

    public async Task<Stream> OpenReadStreamAsync()
    {
        var memoryStream = new MemoryStream();
        await _formFile.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}

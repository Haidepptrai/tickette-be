using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Infrastructure.FileStorage;

public class S3FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileUploadService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"] ?? throw new ApplicationException("Missing AWS S3 Bucket Name");
    }

    public async Task<string> UploadFileAsync(IFileUpload file, string folder)
    {
        if (file.Length == 0)
        {
            throw new ApplicationException("File is empty");
        }

        var key = $"{folder}/{Guid.NewGuid()}_{file.FileName}"; // Use GUID to ensure unique filenames

        try
        {
            using (var stream = await file.OpenReadStreamAsync())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return $"https://{_bucketName}.s3.amazonaws.com/{key}";
                }

                throw new ApplicationException("Failed to upload file to S3.");
            }
        }
        catch (AmazonS3Exception ex)
        {
            throw new ApplicationException("S3 upload failed: " + ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while uploading the file: " + ex.Message, ex);
        }
    }
}
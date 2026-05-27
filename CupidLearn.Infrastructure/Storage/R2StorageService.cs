using Amazon.S3;
using Amazon.S3.Model;
using CupidLearn.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CupidLearn.Infrastructure.Storage;

public class R2StorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly R2Options _options;

    public R2StorageService(IAmazonS3 s3, IOptions<R2Options> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        var key = $"{Guid.NewGuid()}_{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3.PutObjectAsync(request);

        return $"{_options.PublicUrl.TrimEnd('/')}/{key}";
    }

    public async Task DeleteAsync(string fileKey)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileKey
        };

        await _s3.DeleteObjectAsync(request);
    }
}

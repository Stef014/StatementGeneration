using Amazon.S3;
using Amazon.S3.Model;
using StatementGenerationService.Repositories.Interfaces;

namespace StatementGenerationService.Repositories;

public class FileStorageRepository : IFileStorageRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public FileStorageRepository(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["S3BucketName"];
    }

    public async Task<string> UploadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileName = GetKeyFromFilePath(filePath);
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            FilePath = filePath
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return GetPreSignedUrl(fileName, TimeSpan.FromHours(24));
    }

    private string GetKeyFromFilePath(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    private string GetPreSignedUrl(string key, TimeSpan expiryDuration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiryDuration)
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
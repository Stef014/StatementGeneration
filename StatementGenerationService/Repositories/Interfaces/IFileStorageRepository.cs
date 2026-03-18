namespace StatementGenerationService.Repositories.Interfaces;

public interface IFileStorageRepository
{
    Task<string> UploadFileAsync(string filePath, CancellationToken cancellationToken);
}
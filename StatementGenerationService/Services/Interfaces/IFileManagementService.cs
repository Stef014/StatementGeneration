namespace StatementGenerationService.Services.Interfaces;

public interface IFileManagementService
{
    Task<string> UploadFileAsync(string filePath, CancellationToken cancellationToken);
}
using StatementGenerationService.Repositories.Interfaces;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class FileManagementService : IFileManagementService
{
    private readonly IFileStorageRepository _fileStorageRepository;

    public FileManagementService(IFileStorageRepository fileStorageRepository)
    {
        _fileStorageRepository = fileStorageRepository;
    }

    public async Task<string> UploadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        return await _fileStorageRepository.UploadFileAsync(filePath, cancellationToken);
    }
}
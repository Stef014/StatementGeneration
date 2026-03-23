using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class DataDecryptionService : IDataDecryptionService
{
    private readonly IDataProtector _dataProtector;
    private readonly ILogger<DataDecryptionService> _logger;

    public DataDecryptionService(IDataProtectionProvider dataProtectionProvider, ILogger<DataDecryptionService> logger)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("StatementGenerationService.DataDecryption");
        _logger = logger;
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try {
            return _dataProtector.Unprotect(cipherText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting data.");
            throw;
        }
    }
}
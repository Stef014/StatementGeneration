using Microsoft.AspNetCore.DataProtection;
using MonthlyAccountProcessingService.Services.Interfaces;

namespace MonthlyAccountProcessingService.Services;

public class DataEncryptionService : IDataEncryptionService
{
    private readonly IDataProtector _dataProtector;

    public DataEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("MonthlyAccountProcessingService.DataEncryption");
    }

    public string Encrypt(string plainText)
    {
        return _dataProtector.Protect(plainText);
    }
}
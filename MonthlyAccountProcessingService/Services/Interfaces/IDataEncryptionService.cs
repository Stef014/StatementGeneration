namespace MonthlyAccountProcessingService.Services.Interfaces;

public interface IDataEncryptionService
{
    string Encrypt(string plainText);
}
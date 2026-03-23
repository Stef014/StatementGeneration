namespace StatementGenerationService.Services.Interfaces;

public interface IDataDecryptionService
{
    string Decrypt(string cipherText);
}
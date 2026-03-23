using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class DataDecryptionService : IDataDecryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<DataDecryptionService> _logger;

    public DataDecryptionService(IConfiguration configuration, ILogger<DataDecryptionService> logger)
    {
        var keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption:Key is missing from configuration.");
        _key = Convert.FromBase64String(keyString);
        _logger = logger;
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try 
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = _key;
            
            // Extract IV from the beginning
            var iv = new byte[aes.BlockSize / 8];
            var encryptedBytes = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
            
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while decrypting data.");
            throw;
        }
    }
}
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using MonthlyAccountProcessingService.Services.Interfaces;

namespace MonthlyAccountProcessingService.Services;

public class DataEncryptionService : IDataEncryptionService
{
    private readonly byte[] _key;

    public DataEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"] 
            ?? throw new InvalidOperationException("Encryption:Key is missing from configuration.");
        _key = Convert.FromBase64String(keyString);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        // Prepend IV to encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
        
        return Convert.ToBase64String(result);
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text; // Lägg till denna rad
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<EncryptionService>();

var app = builder.Build();

app.MapGet("/encrypt", (HttpContext context, EncryptionService encryptionService) =>
{
    string originalText = "Hemlig information";
    string encryptedText = encryptionService.Encrypt(originalText);
    return context.Response.WriteAsync($"Krypterad text: {encryptedText}");
});

app.MapGet("/decrypt", (HttpContext context, EncryptionService encryptionService) =>
{
    string encryptedText = "Krypterad text här"; // Sätt in den krypterade texten här
    string decryptedText = encryptionService.Decrypt(encryptedText);
    return context.Response.WriteAsync($"Avkrypterad text: {decryptedText}");
});

app.Run();

public class EncryptionService
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwx"); // 256-bit key
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 128-bit IV

    public string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
    }

    public string Decrypt(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}

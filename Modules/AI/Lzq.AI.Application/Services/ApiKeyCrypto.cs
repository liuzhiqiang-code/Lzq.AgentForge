using System.Text;
using Masa.Utils.Security.Cryptography;

namespace Lzq.AI.Application.Services;

public static class ApiKeyCrypto
{
    private static readonly string AesKey = Environment.GetEnvironmentVariable("AES_KEY")
        ?? "AES_KEY";

    /// <summary>
    /// 加密 API Key，返回密文和随机 IV
    /// </summary>
    public static (string CipherText, string IV) Encrypt(string plainApiKey)
    {
        // 随机生成 16 字节 IV，转为 Base64 字符串（Masa 内部会处理字节转换）
        byte[] ivBytes = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(ivBytes);
        string ivString = Convert.ToBase64String(ivBytes);

        // 使用明确的重载：Encrypt(内容, 密钥, IV字节数组)
        string cipher = AesUtils.Encrypt(plainApiKey, AesKey, ivString);
        return (cipher, ivString);
    }

    /// <summary>
    /// 解密，传入密文和加密时使用的 IV 字符串
    /// </summary>
    public static string Decrypt(string cipherText, string ivString)
    {
        return AesUtils.Decrypt(cipherText, AesKey, ivString);
    }
}
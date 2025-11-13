using System.Security.Cryptography;
using System.Text;

namespace ST10448895_CMCS_PROG.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("MySecretEncryptionKey123!@#"); // 32 bytes
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("InitVector1234567"); // 16 bytes

        public static byte[] EncryptFile(byte[] fileBytes)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return PerformCryptography(fileBytes, encryptor);
        }

        public static byte[] DecryptFile(byte[] encryptedBytes)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            return PerformCryptography(encryptedBytes, decryptor);
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}

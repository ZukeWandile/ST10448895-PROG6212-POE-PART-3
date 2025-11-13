using System.Security.Cryptography;
using System.Text;

namespace ST10448895_CMCS_PROG.Helpers
{
    public static class FileEncryptionHelper
    {
        // AES 256 Key & IV (for demo — ideally load from appsettings or Azure KeyVault)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("A5D7F1C9E2B3H6J8K2L4M9P1R5S7T8V0"); // 32 bytes = 256-bit
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1H2G3F4E5D6C7B8A"); // 16 bytes = 128-bit

        public static void EncryptFile(string inputFilePath, string outputFilePath)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
            using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
            using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            inputStream.CopyTo(cryptoStream);
        }

        public static void DecryptFile(string inputFilePath, string outputFilePath)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
            using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

            cryptoStream.CopyTo(outputStream);
        }
    }
}

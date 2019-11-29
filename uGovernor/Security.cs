using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace uGovernor
{
    public unsafe static class Security
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1678;

        private static string Encrypt(string plainText, byte[] passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = GenerateRandomEntropy();
            var ivStringBytes = GenerateRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);

            var keyBytes = password.GetBytes(Keysize / 8);

            using var symmetricKey = new RijndaelManaged();
            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            var cipherTextBytes = saltStringBytes;
            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

            return Convert.ToBase64String(cipherTextBytes);
        }

        private static string Decrypt(string cipherText, byte[] passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);

            var keyBytes = password.GetBytes(Keysize / 8);

            using var symmetricKey = new RijndaelManaged();
            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        private static byte[] GenerateRandomEntropy()
        {
            var randomBytes = new byte[Keysize / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public static void EncryptFile(IDictionary<string, SecureString> settings, string path, byte[] password)
        {
            var dictionary = settings.ToDictionary(x => x.Key, x => x.Value.ToUnsecureString(), StringComparer.OrdinalIgnoreCase);
            var serialized = JsonSerializer.Serialize(dictionary);
            var encrypted = Encrypt(serialized, password);

            File.WriteAllText(path, encrypted);
        }

        public static IDictionary<string, SecureString> DecryptFile(string path, byte[] password)
        {
            var encrypted = File.ReadAllText(path);
            var decrypted = Decrypt(encrypted, password);

            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted);

            return dictionary.ToDictionary(x => x.Key, x => x.Value.ToSecureString(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
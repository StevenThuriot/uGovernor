using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace uGovernor
{
    static class Security
    {
        const int _iterations = 1675;
        const int keySize = sizeof(ushort);

        internal static string Encrypt(string text)
        {
            var salt = CreateSalt();

            byte[] src = Encoding.Unicode.GetBytes(text);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                var key = new Rfc2898DeriveBytes(_fingerPrint.Value, salt, _iterations);

                aes.Key = key.GetBytes(16);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                var prefix = Encoding.ASCII.GetBytes($"{salt.Length}_{aes.IV.Length}_");

                using (var memStream = new MemoryStream())
                {
                    //While a byte would suffice, we'll take the sure-thing and just use a ushort instead.
                    var bytes = BitConverter.GetBytes((ushort)salt.Length);
                    memStream.Write(bytes, 0, bytes.Length);

                    bytes = BitConverter.GetBytes((ushort)aes.IV.Length);
                    memStream.Write(bytes, 0, bytes.Length);
                    
                    memStream.Write(salt, 0, salt.Length);
                    memStream.Write(aes.IV, 0, aes.IV.Length);

                    using (ICryptoTransform encrypt = aes.CreateEncryptor())
                    using (var cryptoStream = new CryptoStream(memStream, encrypt, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(src, 0, src.Length);

                        cryptoStream.FlushFinalBlock();

                        var encrypted = memStream.ToArray();
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
        }

        internal static string Decrypt(string text)
        {
            var src = Convert.FromBase64String(text);

            var saltArray = new byte[keySize];
            Array.Copy(src, saltArray, keySize);
            var saltLength = BitConverter.ToUInt16(saltArray, 0);

            var ivArray = new byte[sizeof(ushort)];
            Array.Copy(src, keySize, ivArray, 0, keySize);
            var ivLength = BitConverter.ToUInt16(ivArray, 0);

            var salt = new byte[saltLength];
            Array.Copy(src, keySize * 2, salt, 0, saltLength);

            var iv = new byte[ivLength];
            var ivStartPosition = keySize * 2 + saltLength;
            Array.Copy(src, ivStartPosition, iv, 0, ivLength);

            var content = new byte[src.Length - keySize * 2 - saltLength - ivLength];
            Array.Copy(src, ivStartPosition + ivLength, content, 0, content.Length);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                var key = new Rfc2898DeriveBytes(_fingerPrint.Value, salt, _iterations);

                aes.Key = key.GetBytes(16);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decrypt = aes.CreateDecryptor())
                {
                    byte[] dest = decrypt.TransformFinalBlock(content, 0, content.Length);
                    return Encoding.Unicode.GetString(dest);
                }
            }
        }




        
        static byte[] CreateSalt(int size = 8)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[size];
                rng.GetBytes(salt);

                return salt;
            }
        }

        static Lazy<string> _fingerPrint = new Lazy<string>(() => 
        {
            var cpu  = Task.Run(() => RunQuery("SELECT UniqueId, ProcessorId, Name, Description, Manufacturer FROM Win32_Processor"));
            var mobo = Task.Run(() => RunQuery("SELECT Manufacturer, Product, Name, SerialNumber FROM Win32_BaseBoard"));
            
            return $"{cpu.Result}>>{mobo.Result}";
        });

        static string RunQuery(string query)
        {
            var qry = new SelectQuery(query);
            var searcher = new ManagementObjectSearcher(qry);
            var result = searcher.Get();

            return result.Cast<ManagementObject>()
                         .First().Properties
                         .Cast<PropertyData>()
                         .Select(x => x.Value)
                         .Where(x => x != null)
                         .Aggregate("", (current, next) => current + next);
        }
    }
}

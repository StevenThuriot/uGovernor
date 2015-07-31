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
                    var underscore = new byte[] { 95 };

                    var bytes = BitConverter.GetBytes(salt.Length);
                    memStream.Write(bytes, 0, bytes.Length);

                    memStream.Write(underscore, 0, underscore.Length);

                    bytes = BitConverter.GetBytes(aes.IV.Length);
                    memStream.Write(bytes, 0, bytes.Length);

                    memStream.Write(underscore, 0, underscore.Length);

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
            const int underscore = '_';

            var src = Convert.FromBase64String(text);
            var saltArray = src.TakeWhile(x => x != underscore).ToArray();
            var saltLength = BitConverter.ToInt32(saltArray, 0);

            var leftover = src.Skip(saltArray.Length + 1).ToArray();
            var ivArray = leftover.TakeWhile(x => x != underscore).ToArray();
            var ivLength = BitConverter.ToInt32(ivArray, 0);


            leftover = leftover.Skip(ivArray.Length + 1).ToArray();
            var salt = leftover.Take(saltLength).ToArray();

            leftover = leftover.ToArray();
            var iv = leftover.Skip(saltLength).Take(ivLength).ToArray();


            var content = src.Skip(saltLength + ivLength + 2 + saltArray.Length + ivArray.Length).ToArray();


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

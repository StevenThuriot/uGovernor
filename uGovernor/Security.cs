using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace uGovernor
{
    static class Security
    {
        const string AesIV256 = @"!QAZ2WSX#EDC4RFV";
        const int _iterations = 1675;
        static byte[] pepper = { 70, 85, 51, 94, 6, 15, 41, 31 };

        internal static string Encrypt(string text)
        {
            byte[] src = Encoding.Unicode.GetBytes(text);

            using (var aes = CreateProvider())
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);
                return Convert.ToBase64String(dest);
            }
        }

        internal static string Decrypt(string text)
        {
            byte[] src = Convert.FromBase64String(text);

            using (var aes = CreateProvider())
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }

        static AesCryptoServiceProvider CreateProvider()
        {
            var aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;

            aes.IV = Encoding.UTF8.GetBytes(AesIV256);

            var key = new Rfc2898DeriveBytes(_fingerPrint.Value, pepper, _iterations);

            aes.Key = key.GetBytes(16);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            return aes;
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

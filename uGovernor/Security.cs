using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;

namespace uGovernor
{
    static class Security
    {
        const int _iterations = 1675;
        const int keySize = sizeof(ushort);

        internal static string Encrypt(string text)
        {
            var salt = CreateSalt();
            
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                var key = new Rfc2898DeriveBytes(_fingerPrint.Value, salt, _iterations);

                aes.Key = key.GetBytes(16);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();

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

                        var length = text.Length * sizeof(char);
                        var @byte = new byte[length];

                        unsafe
                        {
                            fixed (char* ptr = text)
                            {

                                fixed (byte* bytePtr = @byte)
                                {
                                    NativeMethods.memcpy(new IntPtr(bytePtr), new IntPtr(ptr), new UIntPtr((uint)length));
                                }

                                cryptoStream.Write(@byte, 0, length);
                            }
                        }

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

                    unsafe
                    {
                        fixed (byte* b = dest)
                        {
                            var charPtr = (char*)b;
                            var value = new string(charPtr, 0, dest.Length / sizeof(char));
                            return value;
                        }

                    }
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
            const string CPUQry = "SELECT UniqueId, ProcessorId, Name, Description, Manufacturer FROM Win32_Processor";
            const string MoboQry = "SELECT Manufacturer, Product, Name, SerialNumber FROM Win32_BaseBoard";
            
            return $"{RunQuery(CPUQry, MoboQry)}";
        });

        static string RunQuery(params string[] queries)
        {
            var result = queries.AsParallel()
                                .AsSequential()
                                .Select(qry => new SelectQuery(qry))
                                .Select(qry => new ManagementObjectSearcher(qry))
                                .Select(searcher => searcher.Get())
                                .Select(results => results.Cast<ManagementObject>()
                                                          .SelectMany(x => x.Properties.Cast<PropertyData>())
                                                          .Select(x => x.Value)
                                                          .Where(x => x != null)
                                                          .Aggregate("", (current, next) => current + next));

            return string.Join(">>", result);
        }
    }
}

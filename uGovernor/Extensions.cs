using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace uGovernor
{
    static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> GroupPer<T>(this IEnumerable<T> source, int size) 
            => source.Select((Item, Index) => new { Item, Index }).GroupBy(x => x.Index / size, x => x.Item);
    }


    unsafe static class UnsafeExtensions
    {
        public static SecureString ToSecureString(this string password)
        {
            fixed (char* passwordChars = password)
            {
                var securePassword = new SecureString(passwordChars, password.Length);
                securePassword.MakeReadOnly();

                return securePassword;
            }
        }

        public static SecureString ToSecureString(this byte[] password, bool clearArray = true)
        {
            fixed (byte* passwordBytes = password)
            {
                var securePassword = new SecureString((char*)passwordBytes, password.Length / sizeof(char));
                securePassword.MakeReadOnly();

                if (clearArray) Array.Clear(password, 0, password.Length);

                return securePassword;
            }
        }

        public static SecureString ToSecureString(this char[] password, bool clearArray = true)
        {
            fixed (char* passwordBytes = password)
            {
                var securePassword = new SecureString(passwordBytes, password.Length);
                securePassword.MakeReadOnly();

                if (clearArray) Array.Clear(password, 0, password.Length);

                return securePassword;
            }
        }

        public static string ToUnsecureString(this byte[] password, bool clearArray = true)
        {
            unsafe
            {
                fixed (byte* b = password)
                {
                    var charPtr = (char*)b;
                    var value = new string(charPtr, 0, password.Length / sizeof(char));

                    if (clearArray) Array.Clear(password, 0, password.Length);

                    return value;
                }
            }
        }


        public static string ToUnsecureString(this SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = SecureStringMarshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

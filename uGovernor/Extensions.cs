using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace uGovernor
{
    static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> GroupPer<T>(this IEnumerable<T> source, int size)
        {
            return source.Select((Item, Index) => new { Item, Index })
                         .GroupBy(x => x.Index / size, x => x.Item);
        }
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

        public static string ToUnsecureString(this SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}

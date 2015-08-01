using System;
using System.Runtime.InteropServices;
using System.Security;

namespace uGovernor
{
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

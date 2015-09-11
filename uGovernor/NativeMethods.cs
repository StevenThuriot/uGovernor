using System.Runtime.InteropServices;
using System.Security;
using Vault;

namespace uGovernor
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeConsole();



        internal static unsafe byte[] GetBytes(this SecureString value) => GetBytes(value.ToUnsecureString());

        internal static unsafe byte[] GetBytes(this string value)
        {
            var bytes = new byte[value.Length * sizeof(char)];

            fixed (void* v = value, b = bytes)
                UnsafeNativeMethods.memcpy(b, v, bytes.Length);

            return bytes;
        }
    }
}

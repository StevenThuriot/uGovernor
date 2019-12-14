using System.Collections.Generic;
using System.Security;

namespace uGovernor
{
    public interface ISecurity
    {
        IDictionary<string, SecureString> DecryptFile(string path, byte[] password);
        void EncryptFile(IDictionary<string, SecureString> settings, string path, byte[] password);
    }
}
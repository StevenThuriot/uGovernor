using System;
using System.Security;

namespace uGovernor.Domain
{
    interface ISettingsManger
    {
        string Get(string setting);
        SecureString GetSecure(string setting);
        Uri GetUri(string setting);
        void Refresh();
        void Save();
        void Set(string setting, SecureString value);
        void Set(string setting, string value);
        void Set(string setting, Uri value);
    }
}
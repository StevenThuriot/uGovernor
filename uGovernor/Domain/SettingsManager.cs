using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;


namespace uGovernor.Domain
{
    class SettingsManger
#if DEBUG
        : IEnumerable<string>
#endif
    {
        readonly string _path;
        IDictionary<string, SecureString> _settings;

        public SettingsManger(string path, bool init = true)
        {
            _path = path;
            Trace.TraceInformation("Settings location: " + _path);

            if (init)
            {
                Refresh();
            }
            else
            {
                _settings = new Dictionary<string, SecureString>();
            }
        }

        public SettingsManger(bool init = true)
            : this("uGovernor.cfg", init)
        {
        }

        public string Get(string setting)
        {
            SecureString value;
            if (_settings.TryGetValue(setting, out value))
            {
                return value.ToUnsecureString();
            }

            return null;
        }

        public SecureString GetSecure(string setting)
        {
            SecureString value;
            if (_settings.TryGetValue(setting, out value))
            {
                return value;
            }

            return null;
        }

        public Uri GetUri(string setting)
        {
            var uri = Get(setting);
            if (uri == null) return null;

            return new Uri(uri);
        }

        public void Set(string setting, string value)
        {
            _settings[setting] = value.ToSecureString();
        }

        public void Set(string setting, SecureString value)
        {
            Set(setting, value.ToUnsecureString());
        }

        public void Set(string setting, Uri value)
        {
            Set(setting, value.ToString());
        }

        public void Save()
        {
            Trace.TraceInformation("Saving settings to file...");
            Security.EncryptFile(_settings, _path, FingerPrint.Value);
        }

        public void Refresh()
        {
            Trace.TraceInformation("Refreshing settings from file...");
            var password = FingerPrint.Value;

            try
            {
                _settings = Security.DecryptFile(_path, password);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                _settings = new Dictionary<string, SecureString>();
            }
        }

#if DEBUG
        public IEnumerator<string> GetEnumerator() => _settings.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
#endif
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;


namespace uGovernor.Domain
{
    class SettingsManger : ISettingsManger

#if DEBUG
        : IEnumerable<string>
#endif
    {
        readonly string _path = "uGovernor.cfg";

        private readonly IFingerPrint _fingerPrint;
        private readonly ILogger<SettingsManger> _logger;

        IDictionary<string, SecureString> _settings;
        private readonly ISecurity _security;

        IDictionary<string, SecureString> Settings
        {
            get
            {
                if (_settings is null)
                {
                    Refresh();
                }

                return _settings;
            }
        }

        public SettingsManger(ISecurity security, IFingerPrint fingerPrint, ILogger<SettingsManger> logger)
        {
            _fingerPrint = fingerPrint;
            _logger = logger;
            _security = security;
        }

        public string Get(string setting)
        {
            SecureString value;
            if (Settings.TryGetValue(setting, out value))
            {
                return value.ToUnsecureString();
            }

            return null;
        }

        public SecureString GetSecure(string setting)
        {
            SecureString value;
            if (Settings.TryGetValue(setting, out value))
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
            Settings[setting] = value.ToSecureString();
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
            _logger.LogInformation("Saving settings to file...");
            _security.EncryptFile(Settings, _path, _fingerPrint.Get());
        }

        public void Refresh()
        {
            _logger.LogInformation("Refreshing settings from file...");
            var password = _fingerPrint.Get();

            try
            {
                _settings = _security.DecryptFile(_path, password);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                _settings = new Dictionary<string, SecureString>();
            }
        }

#if DEBUG
        public IEnumerator<string> GetEnumerator() => _settings.Keys.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
#endif
    }
}

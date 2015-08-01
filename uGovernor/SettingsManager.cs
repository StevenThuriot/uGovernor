using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;

namespace uGovernor
{
    class SettingsManger
    {
        readonly string _path;
        readonly bool _encrypted;
        IDictionary<string, string> _settings;

        public SettingsManger(string path, bool encrypted = true)
        {
            _path = path;
            _encrypted = encrypted;

            _settings = DecryptFile();
        }

        public SettingsManger(bool encrypted = true)
            : this("uGovernor.cfg", encrypted)
        {
        }

        public string Get(string setting)
        {
            string value;
            if (_settings.TryGetValue(setting, out value))
            {
                if (_encrypted) return Security.Decrypt(value);
                return value;
            }

            return null;
        }

        public SecureString GetSecure(string setting)
        {
            string value;
            if (_settings.TryGetValue(setting, out value))
            {
                return (_encrypted ? Security.Decrypt(value) : value).ToSecureString();
            }

            return null;
        }

        public Uri GetUri(string setting)
        {
            string value;
            if (_settings.TryGetValue(setting, out value))
            {
                var uri = _encrypted ? Security.Decrypt(value) : value;
                return new Uri(uri);
            }

            return null;
        }

        public void Set(string setting, string value)
        {
            _settings[setting] = _encrypted ? Security.Encrypt(value) : value;
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
            EncryptFile(_settings);
        }

        public void Refresh()
        {
            Trace.TraceInformation("Refreshing settings from file...");
            _settings = DecryptFile();
        }

        const char SettingSeparator = '-';
        IDictionary<string, string> DecryptFile()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(_path)) return result;

            var src = File.ReadAllText(_path);
            var lines = Security.Decrypt(src)
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Split(new[] { SettingSeparator }, 2));

            if (_encrypted)
            {
                foreach (var line in lines)
                {
                    var key = line[0];
                    var value = line[1];

                    result[key] = value;
                }
            }
            else
            {
                foreach (var line in lines)
                {
                    var key = line[0];
                    var value = Security.Decrypt(line[1]);

                    result[key] = value;
                }
            }

            return result;
        }

        void EncryptFile(IDictionary<string, string> settings)
        {
            if (!_encrypted)
                foreach (var setting in settings.ToArray()) //current settings aren't encrypted, encrypt before writing.
                    settings[setting.Key] = Security.Encrypt(setting.Value);

            var content = string.Join(Environment.NewLine, settings.Select(x => $"{x.Key}{SettingSeparator}{x.Value}"));
            var encryptedContent = Security.Encrypt(content);

            File.WriteAllText(_path, encryptedContent);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;

namespace uGovernor.Domain
{
    class SettingsManger
    {
        readonly string _path;
        readonly bool _encrypted;
        IDictionary<string, SecureString> _settings;

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
            SecureString value;
            if (_settings.TryGetValue(setting, out value))
            {
                if (!_encrypted)
                    return value.ToUnsecureString();

                return Security.Decrypt(value).ToUnsecureString();
            }

            return null;
        }

        public SecureString GetSecure(string setting)
        {
            SecureString value;
            if (_settings.TryGetValue(setting, out value))
            {
                if (!_encrypted)
                    return value;

                return Security.Decrypt(value).ToSecureString();
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
            _settings[setting] = _encrypted ? Security.Encrypt(value) : value.ToSecureString();
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
        IDictionary<string, SecureString> DecryptFile()
        {
            var result = new Dictionary<string, SecureString>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(_path)) return result;

            var src = File.ReadAllText(_path);

            var bytes = Security.Decrypt(src);
            string decrypted;
            unsafe
            {
                fixed (byte* b = bytes)
                {
                    var charPtr = (char*)b;
                    decrypted = new string(charPtr, 0, bytes.Length / sizeof(char));
                    Array.Clear(bytes, 0, bytes.Length);
                }
            }

            var lines = decrypted.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(x => x.Split(new[] { SettingSeparator }, 2));

            if (_encrypted)
            {
                foreach (var line in lines)
                {
                    var key = line[0];
                    var value = line[1];

                    result[key] = value.ToSecureString();
                }
            }
            else
            {
                foreach (var line in lines)
                {
                    var key = line[0];
                    var value = Security.Decrypt(line[1]).ToSecureString();

                    result[key] = value;
                }
            }

            return result;
        }

        void EncryptFile(IDictionary<string, SecureString> settings)
        {
            if (!_encrypted)
                foreach (var setting in settings.ToArray()) //current settings aren't encrypted, encrypt before writing.
                    settings[setting.Key] = Security.Encrypt(setting.Value);

            var content = string.Join(Environment.NewLine, settings.Select(x => $"{x.Key}{SettingSeparator}{x.Value.ToUnsecureString()}"));
            using (var encryptedContent = Security.Encrypt(content))
            {
                File.WriteAllText(_path, encryptedContent.ToUnsecureString());
            }
        }
    }
}

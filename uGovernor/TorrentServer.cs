using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace uGovernor
{
    public class TorrentServer
    {
        SecureString _password;
        string _username;
        bool _useTokenAuth;

        public Uri Host { get; private set; }

        string _token;
        JavaScriptSerializer _serializer;

        string Token
        {
            get
            {
                if (_token == null && _useTokenAuth)
                {
                    using (var client = CreateService())
                    {
                        var uri = new Uri(Host, "/gui/token.html");
                        var answer = client.DownloadString(uri);
                        
                        var tokenRegex = new Regex("<div id=[\"']token['\"].*?>(?<token>.+)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
                        var match = tokenRegex.Match(answer);
                        if (match.Success)
                        {
                            _token = match.Groups["token"].Value;
                        }
                        else
                        {
                            throw new Exception("Unable to resolve token");
                        }
                    }
                }

                return _token;
            }
        }

        public TorrentServer(Uri host, string username, SecureString password, bool useTokenAuth = true)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));

            Host = host;
            _username = username;
            _password = password;
            _useTokenAuth = useTokenAuth;

            _serializer = new JavaScriptSerializer();
        }

        public Torrent GetTorrent(string hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));

            return new Torrent(this, hash);
        }

        public MultiTorrent GetMultiTorrent(params string[] hashes)
        {
            return GetMultiTorrent(hashes);
        }

        public MultiTorrent GetMultiTorrent(IEnumerable<string> hashes)
        {
            return new MultiTorrent(this, hashes);
        }

        internal IEnumerable<Torrent> GetAllTorrents()
        {
            var reply = Execute("list=1");
                        
            var json = _serializer.Deserialize<Dictionary<string, object>>(reply);
            var torrents = ((ArrayList)json["torrents"])
                                        .Cast<ArrayList>()
                                        .Select(x => (string)x[0]/* first index == hash */)
                                        .Select(x => new Torrent(this, x))
                                        .ToArray();

            return torrents;
        }

        WebClient CreateService()
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential(_username, _password);
            return client;
        }



        internal string ExecuteAction(string action)
        {
            return Execute($"action={action}");
        }

        internal string Execute(string action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (_useTokenAuth) action = $"token={Token}&{action}";
            
            
            string reply;
            using (var client = CreateService())
            {
                Trace.TraceInformation($"Calling server: {action}");

                var uri = new Uri(Host, "/gui/?" + action);
                reply = client.DownloadString(uri);

                if (reply == "invalid request")
                    Trace.TraceError("Invalid request!");
            }

            return reply;
        }
    }
}
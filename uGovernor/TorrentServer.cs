﻿using System;
using System.Collections;
using System.Collections.Generic;
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

        internal IEnumerable<Torrent> GetAllTorrents()
        {
            var reply = Execute("1", "list");
                        
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
        

        internal string Execute(string action, string actionPrefix = "action")
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (actionPrefix == null) throw new ArgumentNullException(nameof(actionPrefix));
            
            if (_useTokenAuth)
            {
                action = $"token={Token}&{actionPrefix}={action}";
            }
            else
            {
                action = $"{actionPrefix}={action}";
            }
            
            string reply;
            using (var client = CreateService())
            {
                var uri = new Uri(Host, "/gui/?" + action);
                reply = client.DownloadString(uri);
            }

            return reply;
        }
    }
}
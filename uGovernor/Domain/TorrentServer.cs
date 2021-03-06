﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Net;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace uGovernor.Domain
{
    public class TorrentServer
    {
        SecureString _password;
        string _username;
        bool _useTokenAuth;

        public Uri Host { get; }

        string _token;
        private readonly ILogger<TorrentServer> _logger;

        string Token
        {
            get
            {
                if (_token == null && _useTokenAuth)
                {
                    using var client = CreateService();
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

                return _token;
            }
        }

        public TorrentServer(Uri host, string username, SecureString password, bool useTokenAuth, ILogger<TorrentServer> logger)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _useTokenAuth = useTokenAuth;
            _logger = logger;
        }

        public Torrent GetTorrent(string hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));

            return new Torrent(this, hash);
        }

        public IEnumerable<Torrent> GetTorrents(IEnumerable<string> hashes)
        {
            if (hashes == null) throw new ArgumentNullException(nameof(hashes));

            return hashes.Select(hash => new Torrent(this, hash)).ToArray();
        }

        internal IEnumerable<Torrent> GetAllTorrents()
        {
            var reply = Execute("list=1");

            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(reply);

            var jsonTorrents = ((JsonElement)json["torrents"]);

            var torrents = jsonTorrents.EnumerateArray()
                                       .Select(x => new Torrent(this, x[0].GetString(), x[2].GetString()))
                                       .ToArray();

            return torrents;
        }

        WebClient CreateService()
        {
            return new WebClient
            {
                Credentials = new NetworkCredential(_username, _password.ToUnsecureString())
            };
        }



        internal string ExecuteAction(string action) => Execute($"action={action}");

        internal string Execute(string action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (_useTokenAuth) action = $"token={Token}&{action}";


            string reply;
            using var client = CreateService();
            _logger.LogInformation($"Calling server: {action}");

            var uri = new Uri(Host, "/gui/?" + action);
            try
            {
                reply = client.DownloadString(uri);
                Thread.Sleep(25); //Wait a bit so we don't hammer.

                return reply;
            }
            catch (WebException)
            {
                _logger.LogError("Invalid request!");
                return "";
            }
        }
    }
}
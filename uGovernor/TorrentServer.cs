using System;
using System.Collections.Generic;
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

        string Token
        {
            get
            {
                if (_token == null)
                {
                    using (var client = CreateService())
                    {
                        var uri = new Uri(Host, "/gui/token.html");
                        var answer = client.DownloadString(uri);
                        
                        var tokenRegex = new Regex(@"<div id=""token"".*?>(?<token>.*+)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
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
        }

        public Torrent GetTorrent(string hash)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));

            return new Torrent(this, hash);
        }

        internal IEnumerable<Torrent> GetAllTorrents()
        {
            var reply = Do("list=1");
            var torrents = reply.torrents;


            throw new NotImplementedException();
        }

        WebClient CreateService()
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential(_username, _password);
            return client;
        }



        internal dynamic Do(string action, bool expectsReply = true)
        {
            if (_useTokenAuth)
            {
                action += "&token=" + Token;
            }

            string reply;
            using (var client = CreateService())
            {
                var uri = new Uri(Host, action);
                reply = client.DownloadString(uri);
            }

            if (!expectsReply) return null;

            var serializer = new JavaScriptSerializer();
            dynamic json = serializer.DeserializeObject(reply);

            return json;
        }
    }
}
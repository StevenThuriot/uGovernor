using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;

namespace uGovernor
{
    class Governor
    {
        public bool AllTorrents { get; private set; }
        public string Hash { get; private set; }
        public TorrentServer Server { get; private set; }
        public IEnumerable<string> Actions { get; private set; }

        /*
        -host xxx
        -user xxx
        -password xxx
        -hash xxxxx
        -all (ignores hash)
        -noTokenAuth
        -start
        -stop
        -force
        -remove
        -removeData
        -add (uses magnet)
        -addResolved (uses torcache)
        */
        public Governor(string[] args)
        {
            bool useToken = true;
            Uri host = null;
            string user = null;
            SecureString password = null;
            var actions = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                var name = args[i].Substring(1).Trim().ToUpperInvariant();

                switch (name)
                {
                    case "HOST":
                        host = new Uri(args[++i]);
                        break;
                    case "USER":
                        user = args[++i];
                        break;

                    case "PASSWORD":
                        var pass = args[++i];
                        password = new SecureString();

                        foreach (var @char in pass)
                            password.AppendChar(@char);
                        break;

                    case "HASH":
                        Hash = args[++i];
                        break;

                    case "ALL":
                        AllTorrents = true;
                        break;

                    case "NOTOKENAUTH":
                        useToken = false;
                        break;

                    default:
                        actions.Add(name);
                        break;
                }
            }
            
            if (user == null || password == null)
            {
                //TODO: Try retrieving from configuration manager instead;
            }

            Server = new TorrentServer(host, user, password, useToken);
            Actions = actions;
        }

        public void Run()
        {
            IEnumerable<Torrent> torrents;

            if (AllTorrents)
            {
                torrents = Server.GetAllTorrents();
            }
            else
            {
                torrents = new Torrent[]
                {
                    Server.GetTorrent(Hash)
                };
            }


            foreach (var torrent in torrents)
                foreach (var action in Actions)
                {
                    Trace.TraceInformation("Executing {0}...", action);
                    switch (action)
                    {
                        case "START":
                            torrent.Start();
                            break;
                        case "STOP":
                            torrent.Stop();
                            break;
                        case "REMOVE":
                            torrent.Remove();
                            break;
                        case "REMOVEDATA":
                            torrent.RemoveData();
                            break;
                        case "FORCE":
                            torrent.Force();
                            break;
                        case "ADD":
                            torrent.Add(true);
                            break;
                        case "ADDRESOLVED":
                            torrent.Add(false);
                            break;



                        default:
                            Trace.TraceError("Unknown action: {0}", action);
                            break;
                    }
                }
        }
    }
}

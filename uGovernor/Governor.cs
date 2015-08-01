using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;


namespace uGovernor
{
    class Governor
    {
        public bool AllTorrents { get; private set; }
        public bool Debug { get; private set; }
        public string Hash { get; private set; }
        public TorrentServer Server { get; private set; }
        public IEnumerable<string> Actions { get; private set; }

        /*
        -host xxx
        -user xxx
        -password xxx
        -hash xxxxx
        -all (ignores hash, also implied if no hash supplied)
        -noTokenAuth
        -debug (writes to debug.log)
        -start[_ifprivate|_ifpublic]
        -stop[_ifprivate|_ifpublic]
        -forcestart[_ifprivate|_ifpublic]
        -unpause[_ifprivate|_ifpublic]
        -pause[_ifprivate|_ifpublic]
        -recheck[_ifprivate|_ifpublic]
        -remove[_ifprivate|_ifpublic]
        -removeData[_ifprivate|_ifpublic]
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
                        password = args[++i].ToSecureString();
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

                    case "DEBUG":
                        Debug = true;
                        break;

                    default:
                        actions.Add(name);
                        break;
                }
            }

            if (Debug)
            {
                var listener = new TextWriterTraceListener("debug.log");
                Trace.Listeners.Add(listener);
            }
            
            if (user == null || password == null || host == null)
            {
                var settings = new SettingsManger();

                if (user == null) user = settings.Get("user");
                if (password == null) password = settings.GetSecure("password");
                if (host == null) host = settings.GetUri("host");
            }

            Server = new TorrentServer(host, user, password, useToken);
            Actions = actions;
        }

        public void Run()
        {
            IEnumerable<Torrent> torrents;

            if (AllTorrents || Hash == null)
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

            const string IFPRIVATE = "_IFPRIVATE";
            const string IFPUBLIC = "_IFPUBLIC";

            foreach (var action in Actions)
            {
                string command;
                
                Execution execution;
                if (action.EndsWith(IFPRIVATE, StringComparison.OrdinalIgnoreCase))
                {
                    command = action.Substring(0, action.Length - IFPRIVATE.Length);
                    execution = Execution.Private;
                }
                else if (action.EndsWith(IFPUBLIC, StringComparison.OrdinalIgnoreCase))
                {
                    command = action.Substring(0, action.Length - IFPUBLIC.Length);
                    execution = Execution.Public;
                }
                else
                {
                    command = action;
                    execution = Execution.Always;
                }

                foreach (var torrent in torrents)
                {
                    Trace.TraceInformation($"Executing {action}...");
                    torrent.Execute(command, execution);
                }
            }
        }

    }
}

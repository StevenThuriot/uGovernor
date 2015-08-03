using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;


namespace uGovernor
{
    class Governor
    {
        public bool AllTorrents { get; }
        public bool Debug { get; }
        public TorrentServer Server { get; }
        public IEnumerable<Command> Actions { get; }
        public IEnumerable<AddCommand> AddActions { get; }
        public IEnumerable<string> Hashes { get; }

        /*
        -host [VALUE]
        -user [VALUE]
        -password [VALUE]

        -hash [VALUE]
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
        -label[_ifprivate|_ifpublic] [VALUE]
        -removeLabel[_ifprivate|_ifpublic]
        -setPrio[_ifprivate|_ifpublic] [VALUE]
        -setProperty[_ifprivate|_ifpublic] [NAME] [VALUE]

        -add (uses magnet)
        -addResolved (uses torcache)
        */
        public Governor(string[] args)
        {
            bool useToken = true;
            Uri host = null;
            string user = null;
            SecureString password = null;
            var actions = new List<Command>();
            var hashes = new List<string>();

            var addTorrents = new List<AddCommand>();

            for (int i = 0; i < args.Length; i++)
            {
                var name = args[i];

                if (!name.StartsWith("-", StringComparison.Ordinal)) continue; //not considered a command

                name = name.Substring(1).Trim().ToUpperInvariant();
                
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
                        hashes.Add(args[++i]);
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

                    case "ADD":
                        addTorrents.Add(new AddCommand(args[++i], true));
                        break;

                    case "ADDRESOLVED":
                        addTorrents.Add(new AddCommand(args[++i], false));
                        break;

                    default:
                        actions.Add(Command.Build(name, ref i, args));
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
            AddActions = addTorrents;
            Actions = actions;
            Hashes = hashes;
        }

        public void Run()
        {
            IEnumerable<Torrent> torrents;

            if (AllTorrents || !Hashes.Any())
            {
                torrents = Server.GetAllTorrents();
            }
            else
            {
                torrents = new Torrent[] {
                    Server.GetMultiTorrent(Hashes)
                };
            }

            var multi = new MultiTorrent(Server, torrents);
            foreach (var action in Actions)            
                action.Run(multi);

            foreach (var action in AddActions)            
                action.Run(Server);
        }
    }
}

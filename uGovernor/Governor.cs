﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;


namespace uGovernor
{
    class Governor
    {
        public TorrentServer Server { get; }
        public IEnumerable<Command> Actions { get; }
        public IEnumerable<AddCommand> AddActions { get; }
        public IEnumerable<string> Hashes { get; }

        /*
        -host [VALUE]
        -user [VALUE]
        -password [VALUE]

        -hash [VALUE] (Can be used multiple times when using the same command on several hashes)
        
        -noTokenAuth
        -debug (appends tracing to debug.log)
        -ui (shows UI, will attach to parent if already in a console)

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

        -save [NAME] [VALUE] [NAME2] [VALUE2] .... [NAME_N] [VALUE_N] (currently used: user, password and host)

        -add [HASH](uses magnet)
        -addResolved [HASH] (uses torcache)
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

                    case "NOTOKENAUTH":
                        useToken = false;
                        break;

                    case "ADD":
                        addTorrents.Add(new AddCommand(args[++i], true));
                        break;

                    case "ADDRESOLVED":
                        addTorrents.Add(new AddCommand(args[++i], false));
                        break;

                    case "DEBUG":
                        var writerListener = new TextWriterTraceListener("debug.log");
                        writerListener.TraceOutputOptions |= TraceOptions.DateTime;
                        Trace.AutoFlush = true; //Otherwise nothing will be written to the file.
                        Trace.Listeners.Add(writerListener);
                        break;

                    case "UI":
                        //Ignore
                        break;


                    default:
                        actions.Add(Command.Build(name, ref i, args));
                        break;
                }
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
            if (!Actions.Any() && !AddActions.Any()) return; //Skip execution if no actions have been declared


            IEnumerable<Torrent> torrents;

            if (Hashes.Any())
            {
                torrents = Server.GetTorrents(Hashes);
            }
            else
            {
                Trace.TraceInformation("Retreiving all torrents;");
                torrents = Server.GetAllTorrents();
                Trace.TraceInformation($"Retrieved {torrents.Count()} torrents;");
            }

            foreach (var action in Actions)
                foreach (var torrentGroup in torrents.GroupPer(30).Select(hashes => new MultiTorrent(Server, hashes)))
                    action.Run(torrentGroup);

            foreach (var action in AddActions)
                action.Run(Server);
        }
    }
}

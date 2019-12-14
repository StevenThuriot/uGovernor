using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Security;
using System.Threading;
using uGovernor.Commands;

namespace uGovernor.Domain
{
    class Governor : IGovernor
    {
        public TorrentServer Server { get; }
        public IEnumerable<ICommand> Actions { get; }
        public IEnumerable<IServerCommand> ServerCommands { get; }
        public IEnumerable<string> Hashes { get; }

        private readonly ILogger<Governor> _logger;

        /*
        -host [VALUE]
        -user [VALUE]
        -password [VALUE]

        -hash [VALUE] (Can be used multiple times when using the same command on several hashes)
        
        -noTokenAuth
        -debug (appends tracing to debug.log)
        -ui (shows UI, will attach to parent if already in a console)
        -list
        -list_ifprivate
        -list_ifpublic


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
        -move[_ifprivate|_ifpublic] [label] [sourceFolder] [file] ( placeholders: %label%, %year%, %month%, %season%, %seasonnr%  )


        -save [NAME] [VALUE] [NAME2] [VALUE2] .... [NAME_N] [VALUE_N] (currently used: user, password and host)

        -add [HASH](uses magnet)
        -addResolved [HASH] (uses torcache)
        */
        public Governor(ISettingsManger settings, IArguments args, ILogger<Governor> logger, ILogger<AddCommand> addLogger, ILogger<ListCommand> listLogger, ILogger<MoveCommand> moveLogger, ILogger<Command> commandLogger, ILogger<TorrentServer> serverLogger)
        {
            _logger = logger;

            bool useToken = true;
            Uri host = null;
            string user = null;
            SecureString password = null;
            var actions = new List<ICommand>();
            var hashes = new List<string>();

            var serverCommands = new List<IServerCommand>();

            for (int i = 0; i < args.Count; i++)
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
                        serverCommands.Add(new AddCommand(args[++i], true, addLogger));
                        break;

                    case "ADDRESOLVED":
                        serverCommands.Add(new AddCommand(args[++i], false, addLogger));
                        break;

                    default:
                        if (name.StartsWith("LIST"))
                        {
                            serverCommands.Add(new ListCommand(ResolveExecutionType(name), listLogger));
                        }
                        else if (name.StartsWith("MOVE"))
                        {
                            string destinationFolder = settings.Get("DESTINATION");

                            var moveCommand = new MoveCommand(destinationFolder: destinationFolder,
                                                              execution: ResolveExecutionType(name),
                                                              label: args[++i],
                                                              sourceFolder: args[++i],
                                                              file: args[++i],
                                                              logger: moveLogger);

                            actions.Add(moveCommand);
                        }
                        else
                        {
                            actions.Add(Command.Build(name, ref i, args, commandLogger));
                        }
                        break;
                }
            }

            if (user == null || password == null || host == null)
            {
                if (user == null) user = settings.Get("USER");
                if (password == null) password = settings.GetSecure("PASSWORD");
                if (host == null) host = settings.GetUri("HOST");
            }

            Server = new TorrentServer(host, user, password, useToken, serverLogger);
            ServerCommands = serverCommands;
            Actions = actions;
            Hashes = hashes;
        }

        private static Execution ResolveExecutionType(string action)
        {
            const string IFPRIVATE = "_IFPRIVATE";
            const string IFPUBLIC = "_IFPUBLIC";

            if (action.EndsWith(IFPRIVATE, StringComparison.OrdinalIgnoreCase))
            {
                return Execution.Private;
            }
            else if (action.EndsWith(IFPUBLIC, StringComparison.OrdinalIgnoreCase))
            {
                return Execution.Public;
            }
            else
            {
                return Execution.Always;
            }
        }

        public void Run()
        {
            if (!Actions.Any() && !ServerCommands.Any()) return; //Skip execution if no actions have been declared

            foreach (var action in ServerCommands)
                action.Run(Server);

            IEnumerable<Torrent> torrents;

            if (Hashes.Any())
            {
                torrents = Server.GetTorrents(Hashes);
            }
            else
            {
                _logger.LogInformation("Retreiving all torrents;");
                torrents = Server.GetAllTorrents();
                _logger.LogInformation($"Retrieved {torrents.Count()} torrents;");
            }

            var torrentGroups = torrents.GroupPer(30).Select(hashes => new MultiTorrent(Server, hashes)).ToArray();

            foreach (var action in Actions)
            {
                foreach (var torrentGroup in torrentGroups)
                    action.Run(torrentGroup);

                Thread.Sleep(125); //Don't hammer inbetween actions
            }
        }
    }
}

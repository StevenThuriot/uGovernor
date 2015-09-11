using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uGovernor.Domain;
using static System.Console;

namespace uGovernor.Commands
{
    class ListCommand : IServerCommand
    {
        readonly Execution _executionLevel;

        public ListCommand(Execution executionLevel)
        {
            _executionLevel = executionLevel;
        }

        [SuppressMessage("SonarQube", "S2228:Console logging should not be used", Justification = "This only needs to be shown when in a console")]
        public void Run(TorrentServer server)
        {
            Program.EnsureShell();

            var torrents = server.GetAllTorrents();

            switch (_executionLevel)
            {
                case Execution.Private:
                    torrents = torrents.Where(x => x.Private);
                    break;

                case Execution.Public:
                    torrents = torrents.Where(x => !x.Private);
                    break;
            }

            WriteLine("");

            if (!torrents.Any())
            {
                WriteLine("No relevant torrents found...");
                return;
            }
            
            var longestName = torrents.Max(x => x.Name.Length);
            var hashLength = torrents.Max(x => x.Hash.Length);

            var alignment = " | {0,-" + longestName + "}";


            WriteLine(" ♦");
            WriteLine(" | Listing torrents...");

            var line = " ♦" + new string('-', longestName + hashLength + 5) + "♦";
            WriteLine(line);

            foreach (var torrent in torrents)
            {
                Write(string.Format(alignment, torrent.Name));
                Write(" : ");                
                Write(torrent.Hash);
                WriteLine(" |");
            }

            WriteLine(line);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uGovernor.Domain;

namespace uGovernor.Commands
{
    class ListCommand : IServerCommand
    {
        readonly Execution _executionLevel;

        public ListCommand(Execution executionLevel)
        {
            _executionLevel = executionLevel;
        }

        public void Run(TorrentServer server)
        {
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

            Trace.WriteLine("");

            if (!torrents.Any())
            {
                Trace.WriteLine("No relevant torrents found...");
                return;
            }
            
            var longestName = torrents.Max(x => x.Name.Length);
            var hashLength = torrents.Max(x => x.Hash.Length);

            var alignment = " | {0,-" + longestName + "}";


            Trace.WriteLine(" ♦");
            Trace.WriteLine(" | Listing torrents...");

            var line = " ♦" + new string('-', longestName + hashLength + 5) + "♦";
            Trace.WriteLine(line);

            foreach (var torrent in torrents)
            {
                Trace.Write(string.Format(alignment, torrent.Name));
                Trace.Write(" : ");                
                Trace.Write(torrent.Hash);
                Trace.WriteLine(" |");
            }

            Trace.WriteLine(line);
        }
    }
}

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

            Console.WriteLine("");

            if (!torrents.Any())
            {
                Console.WriteLine("No relevant torrents found...");
                return;
            }
            
            var longestName = torrents.Max(x => x.Name.Length);
            var hashLength = torrents.Max(x => x.Hash.Length);

            var alignment = " | {0,-" + longestName + "}";


            Console.WriteLine(" ♦");
            Console.WriteLine(" | Listing torrents...");

            var line = " ♦" + new string('-', longestName + hashLength + 5) + "♦";
            Console.WriteLine(line);

            foreach (var torrent in torrents)
            {
                Console.Write(string.Format(alignment, torrent.Name));
                Console.Write(" : ");                
                Console.Write(torrent.Hash);
                Console.WriteLine(" |");
            }

            Console.WriteLine(line);
        }
    }
}

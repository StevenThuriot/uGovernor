using Microsoft.Extensions.Logging;
using System.Linq;
using uGovernor.Domain;

namespace uGovernor.Commands
{
    class ListCommand : IServerCommand
    {
        readonly Execution _executionLevel;
        private readonly ILogger<ListCommand> _logger;

        public ListCommand(Execution executionLevel, ILogger<ListCommand> logger)
        {
            _executionLevel = executionLevel;
            _logger = logger;
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

            if (!torrents.Any())
            {
                _logger.LogInformation("No relevant torrents found...");
                return;
            }

            var longestName = torrents.Max(x => x.Name.Length);
            var hashLength = torrents.Max(x => x.Hash.Length);

            var alignment = " | {0,-" + longestName + "}";


            _logger.LogInformation(" ♦");
            _logger.LogInformation(" | Listing torrents...");

            var line = " ♦" + new string('-', longestName + hashLength + 5) + "♦";
            _logger.LogInformation(line);
            _logger.LogInformation(string.Join(" |", torrents.Select(torrent => $"{string.Format(alignment, torrent.Name)} : {torrent.Hash}")));
            _logger.LogInformation(line);
        }
    }
}

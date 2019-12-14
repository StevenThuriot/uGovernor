using Microsoft.Extensions.Logging;
using System;

using uGovernor.Domain;

namespace uGovernor.Commands
{
    class AddCommand : IServerCommand
    {
        string _hash;
        bool _useMagnet;
        private readonly ILogger<AddCommand> _logger;

        public AddCommand(string hash, bool useMagnet, ILogger<AddCommand> logger)
        {
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _useMagnet = useMagnet;
            _logger = logger;
        }

        public void Run(TorrentServer server)
        {
            string torrentUrl;
            if (_useMagnet)
            {
                torrentUrl = $"magnet:?xt=urn:btih:{_hash}&amp;tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A80%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.publicbt.com%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%2Fannounce";
            }
            else
            {
                torrentUrl = $"http://torcache.net/torrent/{_hash}.torrent";
            }

            _logger.LogInformation($"Adding Torrent: {_hash}");
            server.ExecuteAction($"add-url&s={torrentUrl}");
        }

    }
}

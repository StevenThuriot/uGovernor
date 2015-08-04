using System;
using System.Diagnostics;

namespace uGovernor
{
    class AddCommand
    {
        string _hash;
        bool _useMagnet;

        public AddCommand(string hash, bool useMagnet)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));

            _hash = hash;
            _useMagnet = useMagnet;
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

            Trace.TraceInformation($"Adding Torrent: {_hash}");
            server.ExecuteAction($"add-url&s={torrentUrl}");
        }

    }
}

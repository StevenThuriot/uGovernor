using System;
using System.Net;

namespace uGovernor
{
    public class Torrent
    {
        string _hash;
        TorrentServer _server;

        internal Torrent(TorrentServer torrentServer, string hash)
        {
            if (torrentServer == null) throw new ArgumentNullException(nameof(torrentServer));
            if (hash == null) throw new ArgumentNullException(nameof(hash));


            _server = torrentServer;
            _hash = hash;
        }


        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void RemoveData()
        {
            throw new NotImplementedException();
        }

        public void Force()
        {
            throw new NotImplementedException();
        }

        public void Add(bool useMagnet)
        {
            string torrentUri;
            if (useMagnet)
            {
                torrentUri = "magnet:?xt=urn:btih:" + _hash + "&amp;tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A80%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.publicbt.com%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%2Fannounce";
            }
            else
            {
                torrentUri = "http://torcache.net/torrent/" + _hash + ".torrent";
            }

            //TODO: add to server
            throw new NotImplementedException();
        }

    }
}
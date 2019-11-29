using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace uGovernor.Domain
{
    public class MultiTorrent : Torrent, IReadOnlyList<Torrent>
    {
        IEnumerable<Torrent> _torrents;
        TorrentServer _server;
        string _hashes;

        internal MultiTorrent(TorrentServer server, IEnumerable<Torrent> torrents)
            : base(server, "<MultiTorrentHash>", "<MultiTorrent>")
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (torrents == null || !torrents.Any()) throw new ArgumentNullException(nameof(torrents));

            _server = server;
            _torrents = torrents;
            _hashes = BuildHashes(this);
        }

        internal MultiTorrent(TorrentServer server, IEnumerable<string> hashes)
           : this(server, hashes.Select(hash => server.GetTorrent(hash)).ToArray())
        {
        }

        protected override string CallServer([CallerMemberName] string action = null)
        {
            return _server.ExecuteAction($"{action.ToLowerInvariant()}{_hashes}");
        }

        static string BuildHashes(IEnumerable<Torrent> torrents)
        {
            return torrents.Select(x => $"&hash={x.Hash}").Aggregate("", (current, next) => current + next);
        }

        static string BuildActionParameters(IEnumerable<Torrent> torrents, string actionFormat)
        {
            return torrents.Select(x => string.Format(actionFormat, $"&hash={x.Hash}")).Aggregate("", (current, next) => current + next);
        }

        string CallServer(IEnumerable<Torrent> torrents, [CallerMemberName] string action = null)
        {
            return _server.ExecuteAction($"{action.ToLowerInvariant()}{BuildHashes(torrents)}");
        }


        void EnsureProps()
        {
            var torrentList = _torrents.OfType<IKnowAboutProperties>().Where(x => !x.PropertiesAreSet).Cast<Torrent>().ToDictionary(x => x.Hash);

            var result = CallServer(torrentList.Values, "GetProps");

            foreach (var json in _propertyRegex.Matches(result).Cast<Match>().Select(x => x.Groups["properties"]).Where(x => x.Success).Select(x => x.Value))
            {
                var properties = BuildPropertyDictionary(json);
                var hash = properties["hash"].ToString();

                Torrent torrent = torrentList[hash];
                ((IKnowAboutProperties)torrent).SetProperties(properties);
            }
        }

        IReadOnlyCollection<Torrent> GetExecutableTorrents(Execution execution)
        {
            EnsureProps();
            return _torrents.Where(x => x.CanExecute(execution)).ToArray();
        }




        public override string SetPrio(string prio)
        {
            return SetPrio(this, prio);
        }

        public override string SetPrio(Execution execution, string prio)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return SetPrio(torrents, prio);
        }

        string SetPrio(IEnumerable<Torrent> torrents, string prio)
        {
            var action = BuildActionParameters(torrents, $"{{0}}&p={prio}");
            return _server.ExecuteAction($"setprio{action}");
        }





        public override string SetProperty(string property, string value)
        {
            return SetProperty(this, property, value);
        }

        public override string SetProperty(Execution execution, string property, string value)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return SetProperty(torrents, property, value);
        }

        string SetProperty(IEnumerable<Torrent> torrents, string property, string value)
        {
            var action = BuildActionParameters(torrents, $"&s={property}{{0}}&v={value}");
            return _server.ExecuteAction($"setprops{action}");
        }

        public override string SetLabel(Execution execution, string value)
        {
            return SetProperty(execution, "label", value);
        }

        public override string RemoveLabel(Execution execution)
        {
            return SetProperty(execution, "label", "");
        }


        public IEnumerator<Torrent> GetEnumerator()
        {
            return _torrents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return _hashes.Length;
            }
        }

        public Torrent this[int index]
        {
            get
            {
                return _torrents.ElementAt(index);
            }
        }













        public override bool CanExecute(Execution execution)
        {
            return GetExecutableTorrents(execution).Any();
        }


        public override string ForceStart(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Pause(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Recheck(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Remove(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string RemoveData(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Start(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Stop(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }

        public override string Unpause(Execution execution)
        {
            var torrents = GetExecutableTorrents(execution);
            if (torrents.Count == 0) return null;

            return CallServer(torrents);
        }
    }
}

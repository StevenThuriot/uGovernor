using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;

namespace uGovernor
{
    public class Torrent
    {
        public string Hash { get; private set; }

        TorrentServer _server;

        internal Torrent(TorrentServer torrentServer, string hash)
        {
            if (torrentServer == null) throw new ArgumentNullException(nameof(torrentServer));
            if (hash == null) throw new ArgumentNullException(nameof(hash));


            _server = torrentServer;
            Hash = hash;

            var properties = new Lazy<IReadOnlyDictionary<string, object>>(() => GetProps());
            _private = new Lazy<bool>(() => (bool)properties.Value["Private"]);
            _trackers = new Lazy<IEnumerable<string>>(() => (IEnumerable<string>)properties.Value["Trackers"]);
        }
        
        Lazy<bool> _private;
        public bool Private
        {
            get { return _private.Value; }
        }

        Lazy<IEnumerable<string>> _trackers;
        public IEnumerable<string> Trackers
        {
            get { return _trackers.Value; }
        }


                
        IReadOnlyDictionary<string, object> GetProps()
        {
            var result = CallServer();

            /*
            {"build":25130,
                "props": [{"hash": "678S4DF56S7DG56S7DG56S4DGS5G4SDG564G4564"
                        ,"trackers": "http://tracker1/announce\r\nhttps://tracker2/announce\r\n"
                        ,"ulrate": 0
                        ,"dlrate": 0
                        ,"superseed": 0
                        ,"dht": -1
                        ,"pex": -1
                        ,"seed_override": 0
                        ,"seed_ratio": 1200
                        ,"seed_time": 0
                        ,"ulslots": 0
                        }]
                }

            */

            var serializer = new JavaScriptSerializer();
            var json = serializer.Deserialize<Dictionary<string, object>>(result);
            var props = (Dictionary<string, object>)((ArrayList)json["props"])[0];

            var torrentProperties = new Dictionary<string, object>
            {
                { "Trackers", props["trackers"].ToString().Split(new [] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries) },
                { "Private", props["dht"].Equals(-1) || props["pex"].Equals(-1) },
            };

            return torrentProperties;
        }


        public void Start()
        {
            CallServer();
        }

        public void Stop()
        {
            CallServer();
        }

        public void Remove()
        {
            CallServer();
        }

        public void RemoveData()
        {
            CallServer();
        }

        public void ForceStart()
        {
            CallServer();
        }

        public void Pause()
        {
            CallServer();
        }

        public void Unpause()
        {
            CallServer();
        }

        public void Recheck()
        {
            CallServer();
        }

        string CallServer([CallerMemberName] string action = null)
        {
            return _server.Execute($"{action.ToLowerInvariant()}&hash={Hash}");
        }

        public void SetPrio(string prio)
        {
            _server.Execute($"setprio&hash={Hash}&p={prio}");
        }

        public void Add(bool useMagnet)
        {
            string torrentUrl;
            if (useMagnet)
            {
                torrentUrl = $"magnet:?xt=urn:btih:{Hash}&amp;tr=udp%3A%2F%2Fopen.demonii.com%3A1337%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A80%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.publicbt.com%2Fannounce&amp;tr=udp%3A%2F%2Ftracker.openbittorrent.com%2Fannounce";
            }
            else
            {
                torrentUrl = $"http://torcache.net/torrent/{Hash}.torrent";
            }
            
            _server.Execute($"add-url&s={torrentUrl}");
        }

        public void Execute(string command, Execution execution)
        {
            switch (command)
            {
                case "ADD":
                    Add(true);
                    return;

                case "ADDRESOLVED":
                    Add(false);
                    return;
            }

            if (execution == Execution.Public && Private)
            {
                Trace.TraceInformation($"Skipping {command}: {Hash} is public...");
                return;
            }

            if (execution == Execution.Private && !Private)
            {
                Trace.TraceInformation($"Skipping {command}: {Hash} is private...");
                return;
            }

            switch (command)
            {
                case "START":
                case "STOP":
                case "REMOVE":
                case "REMOVEDATA":
                case "FORCESTART":
                case "PAUSE":
                case "UNPAUSE":
                case "RECHECK":
                    CallServer(command);
                    break;

                default:
                    Trace.TraceError($"Unknown action: {command}");
                    break;
            }
        }
    }
}
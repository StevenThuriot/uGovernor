using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace uGovernor.Domain
{
    public class Torrent : IKnowAboutProperties
    {
        public string Hash { get; private set; }
        public string Name { get; private set; }

        TorrentServer _server;

        internal Torrent(TorrentServer torrentServer, string hash)
            : this(torrentServer, hash, hash)
        {

        }

        internal Torrent(TorrentServer torrentServer, string hash, string name)
        {
            if (torrentServer == null) throw new ArgumentNullException(nameof(torrentServer));
            if (hash == null) throw new ArgumentNullException(nameof(hash));
            if (name == null) throw new ArgumentNullException(nameof(name));


            _server = torrentServer;
            Hash = hash;
            Name = name;
        }

        bool IKnowAboutProperties.PropertiesAreSet
        {
            get { return _properties != null; }
        }

        void IKnowAboutProperties.SetProperties(IReadOnlyDictionary<string, object> properties)
        {
            _properties = properties;
        }


        IReadOnlyDictionary<string, object> _properties;
        protected IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                return _properties ?? (_properties = GetProps());
            }
        }

        public bool Private
        {
            get { return (bool)Properties["Private"]; }
        }
        
        public IEnumerable<string> Trackers
        {
            get { return (IEnumerable<string>)Properties["trackers"]; }
        }
      
        IReadOnlyDictionary<string, object> GetProps()
        {
            var result = CallServer();
            var json = _propertyRegex.Match(result).Groups["properties"].Value;
            return BuildPropertyDictionary(json);
        }

        protected static readonly Regex _propertyRegex = new Regex(@"""props"": \[(?<properties>.*?)\]", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        protected static IReadOnlyDictionary<string, object> BuildPropertyDictionary(string json)
        {
            /*
            {"hash": "678S4DF56S7DG56S7DG56S4DGS5G4SDG564G4564"
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
                        }
                }

            */
            
            var serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<Dictionary<string, object>>(json);


            result["trackers"] = result["trackers"].ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            result["Private"] = result["dht"].Equals(-1) || result["pex"].Equals(-1);

            return result;
        }

        public string Start() => CallServer();

        public string Stop() => CallServer();

        public string Remove() => CallServer();

        public string RemoveData() => CallServer();

        public string ForceStart() => CallServer();

        public string Pause() => CallServer();

        public string Unpause() => CallServer();

        public string Recheck() => CallServer();

        public virtual string SetPrio(string prio) => _server.ExecuteAction($"setprio&hash={Hash}&p={prio}");

        public string SetLabel(string value) => SetProperty("label", value);

        public string RemoveLabel() => SetProperty("label", "");

        public virtual string SetProperty(string property, string value) => _server.ExecuteAction($"setprops&hash={Hash}&s={property}&v={value}");

        public virtual string SetLabel(Execution execution, string value)
        {
            if (CanExecute(execution))
                return SetLabel(value);

            return null;
        }

        public virtual string RemoveLabel(Execution execution)
        {
            if (CanExecute(execution))
                return RemoveLabel();

            return null;
        }

        public virtual string SetPrio(Execution execution, string prio)
        {
            if (CanExecute(execution))
                return SetPrio(prio);

            return null;
        }

        public virtual string SetProperty(Execution execution, string property, string value)
        {
            if (CanExecute(execution))
                return SetProperty(property, value);

            return null;
        }

        protected virtual string CallServer([CallerMemberName] string action = null) => _server.ExecuteAction($"{action.ToLowerInvariant()}&hash={Hash}");





        public virtual bool CanExecute(Execution execution)
        {
            switch (execution)
            {
                case Execution.Private:
                    return Private;

                case Execution.Public:
                    return !Private;

                default: return true;
            }
        }


        public virtual string ForceStart(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Pause(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Recheck(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Remove(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string RemoveData(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Start(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Stop(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }

        public virtual string Unpause(Execution execution)
        {
            if (CanExecute(execution))
                return CallServer();

            return null;
        }
    }
}
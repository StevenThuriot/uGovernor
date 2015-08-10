using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace uGovernor
{
    public class Torrent : IKnowAboutProperties
    {
        public string Hash { get; private set; }

        TorrentServer _server;

        internal Torrent(TorrentServer torrentServer, string hash)
        {
            if (torrentServer == null) throw new ArgumentNullException(nameof(torrentServer));
            if (hash == null) throw new ArgumentNullException(nameof(hash));


            _server = torrentServer;
            Hash = hash;
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

        public string Start()
        {
            return CallServer();
        }

        public string Stop()
        {
            return CallServer();
        }

        public string Remove()
        {
            return CallServer();
        }

        public string RemoveData()
        {
            return CallServer();
        }

        public string ForceStart()
        {
            return CallServer();
        }

        public string Pause()
        {
            return CallServer();
        }

        public string Unpause()
        {
            return CallServer();
        }

        public string Recheck()
        {
            return CallServer();
        }

        public virtual string SetPrio(string prio)
        {
            return _server.ExecuteAction($"setprio&hash={Hash}&p={prio}");
        }

        public string SetLabel(string value)
        {
            return SetProperty("label", value);
        }

        public string RemoveLabel()
        {
            return SetProperty("label", "");
        }

        public virtual string SetProperty(string property, string value)
        {
            return _server.ExecuteAction($"setprops&hash={Hash}&s={property}&v={value}");
        }

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

        protected virtual string CallServer([CallerMemberName] string action = null)
        {
            return _server.ExecuteAction($"{action.ToLowerInvariant()}&hash={Hash}");
        }





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
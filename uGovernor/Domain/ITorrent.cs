using System.Collections.Generic;

namespace uGovernor.Domain
{
    interface IKnowAboutProperties
    {
        bool PropertiesAreSet { get; }

        void SetProperties(IReadOnlyDictionary<string, object> properties);
    }
}
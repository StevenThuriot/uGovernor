using System.Collections.Generic;

namespace uGovernor
{
    interface IKnowAboutProperties
    {
        bool PropertiesAreSet { get; }

        void SetProperties(IReadOnlyDictionary<string, object> properties);
    }
}
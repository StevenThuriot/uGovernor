using System.Collections.Generic;

namespace uGovernor
{
    interface IArguments : IEnumerable<string>
    {
        public int Count { get; }
        public string this[int index] { get; }
    }

    class Arguments : List<string>, IArguments
    {
        public Arguments(IEnumerable<string> args)
            : base (args)
        {

        }
    }
}

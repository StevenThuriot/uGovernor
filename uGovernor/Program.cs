using System.Diagnostics;

namespace uGovernor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Trace.TraceError("No startup arguments supplied.", args);
                return;
            }

            var governor = new Governor(args);
            governor.Run();
        }
    }
}

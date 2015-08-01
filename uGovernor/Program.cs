using System;
using System.Diagnostics;
using System.Linq;

namespace uGovernor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Trace.TraceError("No startup arguments supplied.", args);
                Environment.Exit(1);
            }
            else if (args.Contains("-SAVE", StringComparer.OrdinalIgnoreCase))
            {
                var settings = new SettingsManger(false);

                var commands = args.Where(x => !StringComparer.OrdinalIgnoreCase.Equals(x, "-SAVE"))
                                   .Select((x,i) => i % 2 == 0 ? x.TrimStart('-').ToUpperInvariant() : x)
                                   .ToArray();

                for (int i = 0; i < commands.Length; i++)
                    settings.Set(commands[i], commands[++i]);

                settings.Save();
            }
            else
            {
                var governor = new Governor(args);
                governor.Run();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using uGovernor.Domain;

namespace uGovernor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProfileOptimization.SetProfileRoot(AppDomain.CurrentDomain.BaseDirectory);
            ProfileOptimization.StartProfile("uGovernor.jit");
            
            if (args == null || args.Length == 0)
            {
                Trace.TraceError("No startup arguments supplied.", args);
                Environment.Exit(1);
                return;
            }

            var enableUI = args.Contains("-ui", StringComparer.OrdinalIgnoreCase) || args.Any(x => x.StartsWith("-list", StringComparison.OrdinalIgnoreCase));
            bool attached = false;
            
                        
            if (enableUI)
            {
                if (NativeMethods.AttachConsole(-1)) //Try to attach to parent. Useful if already running in a console
                {
                    attached = true;
                }
                else          
                {
                    enableUI = NativeMethods.AllocConsole();
                }
            }

            if (enableUI)
            {
                var consoleListener = new ConsoleTraceListener();
                consoleListener.TraceOutputOptions |= TraceOptions.DateTime | TraceOptions.Timestamp;

                Trace.Listeners.Add(consoleListener);
            }
            
            try
            {
                if (args.Contains("-SAVE", StringComparer.OrdinalIgnoreCase))
                {
                    var settings = new SettingsManger(false);

                    var commands = args.Where(x => !StringComparer.OrdinalIgnoreCase.Equals(x, "-SAVE"))
                                       .Select((x, i) => i % 2 == 0 ? x.TrimStart('-').ToUpperInvariant() : x)
                                       .ToArray();

                    for (int i = 0; i < commands.Length; i++)
                        settings.Set(commands[i], commands[++i]);

                    Trace.TraceInformation("Saving settings...");
                    settings.Save();
                }
                else
                {
                    var governor = new Governor(args);
                    governor.Run();
                }
            }
            finally
            {
                if (enableUI)
                {
                    Console.Write("Press any key to continue...");

                    if (!attached)
                    {
                        Console.ReadKey(true);
                    }

                    NativeMethods.FreeConsole();
                }
            }
        }
    }
}

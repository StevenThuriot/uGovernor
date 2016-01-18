using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using uGovernor.Domain;
using static System.Console;

namespace uGovernor
{
    public static class Program
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

            Trace.TraceInformation(string.Join("~", args));

            _enableUI = args.Contains("-ui", StringComparer.OrdinalIgnoreCase);
            _attached = false;


            if (_enableUI && EnsureShell())
            {
                var consoleListener = new ConsoleTraceListener();
                consoleListener.TraceOutputOptions |= TraceOptions.DateTime | TraceOptions.Timestamp;

                Trace.Listeners.Add(consoleListener);
            }
            
            try
            {
                if (args.Contains("-SAVE", StringComparer.OrdinalIgnoreCase))
                {
                    var settings = new SettingsManger();

                    var commands = args.Where(x => !StringComparer.OrdinalIgnoreCase.Equals(x, "-SAVE"))
                                       .Select((x, i) => i % 2 == 0 ? x.TrimStart('-').ToUpperInvariant() : x)
                                       .ToArray();

                    for (int i = 0; i < commands.Length; i++)
                        settings.Set(commands[i], commands[++i]);

                    Trace.TraceInformation("Saving settings...");
                    settings.Save();
                }
#if DEBUG
                else if (args.Contains("-PRINTCFG", StringComparer.OrdinalIgnoreCase))
                {
                    EnsureShell();

                    var settings = new SettingsManger();
                    foreach (var setting in settings)
                    {
                        Console.WriteLine($"{setting} - {settings.Get(setting)}");
                    }                    
                }
#endif
                else
                {
                    var governor = new Governor(args);
                    governor.Run();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("{0}{0}{1}{0}", Environment.NewLine, e.Message));

                if (_attached)
                    ReadKey();
            }
            finally
            {
                if (_enableUI)
                {
                    Write("Press any key to continue...");

                    if (!_attached)
                        ReadKey(true);

                    NativeMethods.FreeConsole();
                }
            }
        }

        static bool _shellEnsured;
        static bool _enableUI;
        static bool _attached;

        internal static bool EnsureShell()
        {
            if (!_shellEnsured)
            {

                if (NativeMethods.AttachConsole(-1)) //Try to attach to parent. Useful if already running in a console
                {
                    _attached = true;
                }
                else
                {
                    _enableUI = NativeMethods.AllocConsole();
                }

                _shellEnsured = true;
            }

            return _enableUI;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using uGovernor.Domain;

namespace uGovernor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                args = new[] { "-ui", "-list" };
            }

            if (args.Contains("-debug", StringComparer.OrdinalIgnoreCase))
            {
                var writerListener = new TextWriterTraceListener(File.Open(Path.Combine("debug.log"), FileMode.Append));
                writerListener.TraceOutputOptions |= TraceOptions.DateTime;
                Trace.AutoFlush = true; //Otherwise nothing will be written to the file.
                Trace.Listeners.Add(writerListener);

                args = args.Except(new[] { "-debug" }, StringComparer.OrdinalIgnoreCase).ToArray();

                Trace.TraceInformation("Attached debug log");
            }

            if (args.Contains("-ui", StringComparer.OrdinalIgnoreCase))
            {
                args = args.Except(new[] { "-ui" }, StringComparer.OrdinalIgnoreCase).ToArray();

                var consoleListener = new ConsoleTraceListener();
                consoleListener.TraceOutputOptions |= TraceOptions.DateTime | TraceOptions.Timestamp;

                Trace.Listeners.Add(consoleListener);

                Trace.TraceInformation("Attached console log");
            }

            Trace.TraceInformation(string.Join("~", args));

            try
            {
                if (args.Contains("-SAVE", StringComparer.OrdinalIgnoreCase))
                {
                    var settings = new SettingsManger(init: false);

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
            catch (Exception e)
            {
                Trace.TraceError(string.Format("{0}{0}{1}{0}", Environment.NewLine, e));
            }
            finally
            {
                Trace.TraceInformation("Shutting down\n\n=============================\n\n");

                foreach (var disposable in Trace.Listeners.OfType<IDisposable>())
                {
                    disposable.Dispose();
                }
            }
        }
    }
}

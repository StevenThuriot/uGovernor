using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NReco.Logging.File;
using System;
using System.Linq;
using uGovernor.Domain;

namespace uGovernor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                args = new[] { "-ui", "-list" };
            }

            bool includeDebug = args.Contains("-debug", StringComparer.OrdinalIgnoreCase);
            bool showUI = args.Contains("-ui", StringComparer.OrdinalIgnoreCase);

            if (includeDebug)
            {
                args = args.Except(new[] { "-debug" }, StringComparer.OrdinalIgnoreCase).ToArray();
            }

            if (showUI)
            {
                args = args.Except(new[] { "-ui" }, StringComparer.OrdinalIgnoreCase).ToArray();
            }

            var provider = ResolveServiceProvider(args, includeDebug, showUI);
            var logger = provider.GetService<ILogger<Program>>();

            logger.LogInformation(string.Join("~", args));

            try
            {
                if (args.Contains("-SAVE", StringComparer.OrdinalIgnoreCase))
                {
                    var settings = provider.GetService<ISettingsManger>();

                    var commands = args.Where(x => !StringComparer.OrdinalIgnoreCase.Equals(x, "-SAVE"))
                                       .Select((x, i) => i % 2 == 0 ? x.TrimStart('-').ToUpperInvariant() : x)
                                       .ToArray();

                    for (int i = 0; i < commands.Length; i++)
                        settings.Set(commands[i], commands[++i]);

                    logger.LogInformation("Saving settings...");
                    settings.Save();
                }
                else
                {
                    provider.GetService<IGovernor>().Run();
                }
            }
            catch (Exception e)
            {
                logger.LogError(string.Format("{0}{0}{1}{0}", Environment.NewLine, e));
            }
            finally
            {
                logger.LogInformation("Shutting down\n\n=============================\n\n");
            }
        }

        private static ServiceProvider ResolveServiceProvider(string[] args, bool debug, bool ui)
        {
            var logLevel = debug ? LogLevel.Debug : LogLevel.Information;
            var serviceProvider = new ServiceCollection()
                                        .AddLogging(x =>
                                        {
                                            if (ui)
                                            {
                                                x.AddConsole(c =>
                                                 {
                                                     c.Format = ConsoleLoggerFormat.Systemd;
                                                     c.LogToStandardErrorThreshold = logLevel;
                                                 });
                                            }

                                            if (debug)
                                            {
                                                x.AddProvider(new FileLoggerProvider("debug.log"));
                                            }
                                        })
                                        .AddSingleton<ISecurity, Security>()
                                        .AddSingleton<IFingerPrint, FingerPrint>()
                                        .AddSingleton<ISettingsManger, SettingsManger>()
                                        .AddSingleton<IGovernor, Governor>()
                                        .AddSingleton<IArguments>(new Arguments(args))
                                        .BuildServiceProvider();

            return serviceProvider;
        }
    }
}

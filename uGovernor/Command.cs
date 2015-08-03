using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace uGovernor
{
    class Command
    {
        public string Action { get; }
        public IReadOnlyList<string> Arguments { get; }
        public Execution ExecutionLevel { get; }

        public Command(string command, IReadOnlyList<string> arguments, Execution executionLevel)
        {
            Action = command;
            Arguments = arguments;
            ExecutionLevel = executionLevel;
        }

        const string IFPRIVATE = "_IFPRIVATE";
        const string IFPUBLIC = "_IFPUBLIC";

        internal static Command Build(string action, ref int i, string[] args)
        {
            string command;
            var arguments = new List<string>();
            Execution execution;
            
            if (action.EndsWith(IFPRIVATE, StringComparison.OrdinalIgnoreCase))
            {
                command = action.Substring(0, action.Length - IFPRIVATE.Length);
                execution = Execution.Private;
            }
            else if (action.EndsWith(IFPUBLIC, StringComparison.OrdinalIgnoreCase))
            {
                command = action.Substring(0, action.Length - IFPUBLIC.Length);
                execution = Execution.Public;
            }
            else
            {
                command = action;
                execution = Execution.Always;
            }
            
            while (args.Length > i+1)
            {
                var argument = args[i + 1];
                if (argument.StartsWith("-", StringComparison.Ordinal)) break; //not an argument but the next command

                arguments.Add(argument);

                i++;
            }


            return new Command(command, arguments, execution);
        }


        public void Run(Torrent torrent)
        {
            Trace.TraceInformation($"Running action {Action}...");

            switch (Action)
            {
                case "START":
                    torrent.Start(ExecutionLevel);
                    break;

                case "STOP":
                    torrent.Stop(ExecutionLevel);
                    break;

                case "REMOVE":
                    torrent.Remove(ExecutionLevel);
                    break;

                case "REMOVEDATA":
                    torrent.RemoveData(ExecutionLevel);
                    break;

                case "FORCESTART":
                    torrent.ForceStart(ExecutionLevel);
                    break;

                case "PAUSE":
                    torrent.Pause(ExecutionLevel);
                    break;

                case "UNPAUSE":
                    torrent.Unpause(ExecutionLevel);
                    break;

                case "RECHECK":
                    torrent.Recheck(ExecutionLevel);
                    break;

                case "SETPRIO":
                    torrent.SetPrio(ExecutionLevel, Arguments[0]);
                    break;

                case "LABEL":
                    torrent.SetLabel(ExecutionLevel, Arguments[0]);
                    break;

                case "REMOVELABEL":
                    torrent.RemoveLabel(ExecutionLevel);
                    break;

                case "SETPROPERTY":
                    torrent.SetProperty(ExecutionLevel, Arguments[0], Arguments[1]);
                    break;

                default:
                    Trace.TraceError($"Unknown action: {Action}");
                    break;
            }
        }
    }
}

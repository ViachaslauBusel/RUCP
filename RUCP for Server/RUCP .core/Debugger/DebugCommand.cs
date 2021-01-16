using RUCP.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    internal static class DebugCommand
    {
        static DebugCommand()
        {
           
        }
        internal static void Command(Queue<string> commands)
        {

            commands.TryDequeue(out string command);
            commands.TryDequeue(out string storage);
            int count = int.MaxValue;
            if (commands.TryDequeue(out string strCount)
            && int.TryParse(strCount, out int c))
            {
                count = c;
            }

            switch (command)
            {
                case "help":
                    Terminal.PrintHelp("debug -s error 0", "Outputting exceptions to the console. 0 - Optional parametr(Count)");
                    Terminal.PrintHelp("debug -r error 0", "Removing exceptions from storage. 0 - Optional parametr(Count)");
                    Terminal.PrintHelp("debug -s message 0", "Outputting messages to the console. 0 - Optional parametr(Count)");
                    Terminal.PrintHelp("debug -r message 0", "Removing messages from storage. 0 - Optional parametr(Count)");
                    break;
                case "-s": Show(storage, count);
                    break;
                case "-r": Remove(storage, count);
                    break;
                case "-rs":
                case "-sr":
                    Show(storage, count);
                    Remove(storage, count);
                    break;
                default:
                    System.Console.WriteLine("Command not found. Please use 'debug help' command");
                    break;
            }
        }

        private static void Remove(string storage, int count)
        {
            int deleted = 0;
            switch (storage)
            {
                case "error":
                    while (count-- > 0 && Debug.Errors.TryDequeue(out Exception e)) deleted++;
                    System.Console.WriteLine($"deleted {deleted} exceptions. available for deletion {Debug.Errors.Count}");
                    break;
                case "message":
                    while (count-- > 0 && Debug.Messages.TryDequeue(out Message e)) deleted++;
                    System.Console.WriteLine($"deleted {deleted} messages. available for deletion {Debug.Messages.Count}");
                    break;
                default:
                    System.Console.WriteLine("Wrong command");
                    break;
            }
        }
        private static void Show(string storage, int count)
        {
            switch (storage)
            {
                case "error":
                    System.Console.WriteLine($"{Debug.Errors.Count} exceptions available for output");
                    foreach (Exception e in Debug.Errors)
                    {
                        if (count-- <= 0) break;
                        Terminal.PrintError(e.ToString());
                    }
                    break;
                case "message":
                    System.Console.WriteLine($"{Debug.Messages.Count} messages available for output");
                    foreach (Message e in Debug.Messages)
                    {
                        if (count-- <= 0) break;
                        System.Console.WriteLine(e.ToString());
                    }
                    break;
                default:
                    System.Console.WriteLine("Wrong command");
                    break;
            }
        }

    }
}

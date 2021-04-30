using RUCPs.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Debugger
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
                    Terminal.PrintHelp("debug -s 0", "Outputting logs to the console. 0 - Optional parametr(Count)");
                    Terminal.PrintHelp("debug -r 0", "Removing logs from storage. 0 - Optional parametr(Count)");
                    Terminal.PrintHelp("debug enter", "Monitor Real-time logs");
                    break;
                case "-s": Show(count);
                    break;
                case "-r": Remove(count);
                    break;
                case "-rs":
                case "-sr":
                    Show(count);
                    Remove(count);
                    break;
                case "enter":
                    Enter();
                    break;
                default:
                    System.Console.WriteLine("Command not found. Please use 'debug help' command");
                    break;
            }
        }
        private static void Enter()
        {
            System.Console.WriteLine("You entered log monitoring mode. To exit press q");
            Action<Note> print = (m) => m.Print();
            Debug.log += print;
            while (true)
            {
                ConsoleKeyInfo _c = System.Console.ReadKey(true);
                if (_c.KeyChar == 'q' || _c.KeyChar == 'Q')
                {
                    Debug.log -= print;
                    System.Console.WriteLine("You exited monitoring mode");
                    break;
                }
            }
        }
        private static void Remove(int count)
        {
            int deleted = 0;

                    while (count-- > 0 && Debug.Messages.TryDequeue(out Note e)) deleted++;
                    System.Console.WriteLine($"deleted {deleted} logs. available for deletion {Debug.Messages.Count}");
        }
        private static void Show(int count)
        {

            System.Console.WriteLine($"{Debug.Messages.Count} logs available for output");
            foreach (Note e in Debug.Messages)
            {
                if (count-- <= 0) break;
                e.Print();
            }
        }

    }
}

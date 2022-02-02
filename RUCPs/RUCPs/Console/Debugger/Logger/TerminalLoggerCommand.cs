using RUCPs.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Logger
{
    public class TerminalLoggerCommand : ICommand
    {
        private TerminalLogger m_logger;

        public string Description { get; init; }

        public string Name { get; init; }

        public TerminalLoggerCommand(TerminalLogger logger, string commandName = "debug", string description = "Working with the log")
        {
            m_logger = logger;
            Name = commandName;
            Description = description;
        }
        void ICommand.Process(Queue<string> commands)
        {

            commands.TryDequeue(out string command);
         //   commands.TryDequeue(out string storage);

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
        private void Enter()
        {
            System.Console.WriteLine("You entered log monitoring mode. To exit press q");
            m_logger.print += Print;
            while (true)
            {
                ConsoleKeyInfo _c = System.Console.ReadKey(true);
                if (_c.KeyChar == 'q' || _c.KeyChar == 'Q')
                {
                    m_logger.print -= Print;
                    System.Console.WriteLine("You exited monitoring mode");
                    break;
                }
            }
        }
        private void Remove(int count)
        {
            int deleted = m_logger.RemoveFirstMessages(count);
                    System.Console.WriteLine($"deleted {deleted} logs. available for deletion {m_logger.MessagesCount}");
        }
        private void Show(int count)
        {

            System.Console.WriteLine($"{m_logger.MessagesCount} logs available for output");
            foreach (Note e in m_logger.GetMessagesEnumerable())
            {
                if (count-- <= 0) break;
                Print(e);
            }
        }
        private void Print(Note note) 
        {
            ConsoleColor color = note.Type switch
            {
                MsgType.Debug => ConsoleColor.White,
                MsgType.INFO => ConsoleColor.Blue,
                MsgType.WARNING => ConsoleColor.Yellow,
                MsgType.ERROR => ConsoleColor.Red,
                MsgType.FATAL => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
            Terminal.PrintLine(note.FormatMessage(m_logger.Format), color);
        }

  
    }
}

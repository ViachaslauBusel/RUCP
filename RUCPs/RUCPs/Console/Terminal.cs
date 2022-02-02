/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCPs.Cryptography;
using RUCPs.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Console
{
    public static class Terminal
    {

        private static Dictionary<string, ICommand> m_commands = new Dictionary<string, ICommand>();
        static Terminal()
        {
            m_commands.Add("help", new Command("help", "Display information about builtin commands", (q) =>
            {
                foreach (KeyValuePair<string, ICommand> c in m_commands)
                { PrintHelp(c.Value.Name, c.Value.Description); }
            }));

            m_commands.Add("online", new Command("online", "Number of connected clients", (q) => System.Console.WriteLine($"online: {ClientList.online()}")));
            m_commands.Add("keygen", new Command("keygen", "Generate a key to establish a secure connection", (q) => ContainerRSAKey.GenerateKey()));
            m_commands.Add("exit", new Command("exit", "Finish working with the terminal", (q) => throw new TerminalException()));
        }
        public static void Listen()
        {
            while (true)
            {
                try
                {
                    Print("Server: ", ConsoleColor.Green);

                    string[] commandArray = System.Console.ReadLine().Split(" ");
                    for (int i = 0; i < commandArray.Length; i++) commandArray[i] = commandArray[i].Trim().ToLower();
                    Queue<string> command = new Queue<string>(commandArray);

                    if (command.TryDequeue(out string strtCommand)
                    && m_commands.ContainsKey(strtCommand))
                    { m_commands[strtCommand].Process(command); }
                    else System.Console.WriteLine("Command not found. Please use 'help' command");
                }
                catch(TerminalException) { return; }
                catch (Exception e) { Server.CallException(e); }

            }
        }

        public static void AddCommand(ICommand command)
        {
            if (m_commands.ContainsKey(command.Name))
            { m_commands[command.Name] = command; }
            else
            { m_commands.Add(command.Name, command); }
        }

        internal static void PrintHelp(string name, string description)
        {
            if (string.IsNullOrEmpty(description)) return;
            Print(name, ConsoleColor.Blue);
            System.Console.WriteLine($"  {description}");
        }
        public static void PrintLine(string msg, ConsoleColor color)
        {
            ConsoleColor defaultColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(msg);
            System.Console.ForegroundColor = defaultColor;
        }
        public static void Print(string msg, ConsoleColor color)
        {
            ConsoleColor defaultColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.Write(msg);
            System.Console.ForegroundColor = defaultColor;
        }
        public static void UpdatePrint(string msg, ConsoleColor color)
        {
            Print(msg, color);
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
        }
    }
}

/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using RUCP.Cryptography;
using RUCP.Debugger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Console
{
    public class Terminal
    {
     //   private Server server;
        private Dictionary<string, Command> commands = new Dictionary<string, Command>();
        public Terminal(Server server)
        {
           // this.server = server;
            commands.Add("help", new Command("help", "Display information about builtin commands", (q) =>
            {
                foreach(KeyValuePair<string, Command> c in commands)
                        PrintHelp(c.Value.Name, c.Value.Description);
            }));
            commands.Add("stop", new Command("stop", "The command brings the server down in a secure way", (q) => server.Stop()));
            commands.Add("start", new Command("start", "The command starts the server", (q) => server.Start()));
            commands.Add("restart", new Command("restart", "The command restart the server", (q) => { server.Stop(); server.Start(); }));
            commands.Add("debug", new Command("debug", "Working with exceptions and messages", (q) => DebugCommand.Command(q)));
            commands.Add("online", new Command("online", "Number of connected clients", (q) => System.Console.WriteLine($"online: {ClientList.online()}")));
            commands.Add("keygen", new Command("keygen", "Generate a key to establish a secure connection", (q) => ContainerRSAKey.GenerateKey()));
            commands.Add("exit", new Command("exit", "Finish working with the terminal", (q) => throw new TerminalException()));
        }
        public void Listen()
        {
            while (true)
            {
                try
                {
                    ConsoleColor defaultColor = System.Console.ForegroundColor;
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write("Server: ");
                    System.Console.ForegroundColor = defaultColor;

                    string[] commandArray = System.Console.ReadLine().Split(" ");
                    for (int i = 0; i < commandArray.Length; i++) commandArray[i] = commandArray[i].Trim().ToLower();
                    Queue<string> command = new Queue<string>(commandArray);

                    if (command.TryDequeue(out string strtCommand)
                    && commands.ContainsKey(strtCommand))
                        commands[strtCommand].Invoke(command);
                    else System.Console.WriteLine("Command not found. Please use 'help' command");
                }
                catch(TerminalException) { return; }
                catch (Exception e) { Debug.Log(e); }

            }
        }

        public void AddCommand(Command command)
        {
            if (commands.ContainsKey(command.Name))
            { commands[command.Name] = command; }
            else
            { commands.Add(command.Name, command); }
        }

        internal static void PrintHelp(string name, string description)
        {
            if (string.IsNullOrEmpty(description)) return;
            ConsoleColor defaultColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.Write(name);
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine($"  {description}");
        }
        internal static void PrintError(string msg)
        {
            Print(msg, ConsoleColor.Red);
        }
        internal static void Print(string msg, ConsoleColor color)
        {
            ConsoleColor defaultColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(msg);
            System.Console.ForegroundColor = defaultColor;
        }
    }
}

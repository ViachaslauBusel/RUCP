
using RUCPs.Console;
using System;
using System.Collections.Generic;
using System.IO;

namespace RUCPs.Debugger.Monitoring
{
    public class TelemetryCommand : ICommand
    {
        private Telemetry m_telemetry;
        public string Description { get; init; }
        /// <summary>filename to write data table</summary>
        public string FileName { get; set; } = "telemetry";

        public string Name { get; init; }

        public TelemetryCommand(Telemetry telemetry, string commandName="telemetry", string description = "Working with data monitoring")
        {
            m_telemetry = telemetry;
            Name = commandName;
            Description = description;
        }
        void ICommand.Process(Queue<string> commands)
        {

            commands.TryDequeue(out string command);

            //int count = int.MaxValue;
            //if (commands.TryDequeue(out string strCount)
            //&& int.TryParse(strCount, out int c))
            //{
            //    count = c;
            //}

            switch (command)
            {
                case "help":
                    Terminal.PrintHelp("telemetry -s", "Outputting a table to a console.");
                    Terminal.PrintHelp("telemetry -w", "Outputting a table to a file.");
                    //   Terminal.PrintHelp("telemetry enter", "Monitor Real-time logs");
                    break;
                case "-s":
                    Show();
                    break;
                case "-w":
                    Write();
                    break;
                default:
                    System.Console.WriteLine("Command not found. Please use 'telemetry help' command");
                    break;
            }
        }
       private void Write()
        {
            if (!Directory.Exists(@"Logs")) { Directory.CreateDirectory(@"Logs"); }
            string file = $"{FileName}_{DateTime.UtcNow.Date}";
            using (TextWriter stream_out = File.CreateText(Path.Combine(@"Logs/", $"{FileName}.log")))
            {
                string[] table = CreateTable().GetText();
                foreach(string row in table) { stream_out.WriteLine(row); }
            }
            System.Console.WriteLine($"{m_telemetry.ProbesCount} probe were recorded in a file {FileName}");
        }
      
        private void Show()
        {

            System.Console.WriteLine($"{m_telemetry.ProbesCount} probe available for output");
            var table = CreateTable();
            table.Draw();

        }

        private ConsoleTable CreateTable()
        {
            var table = new ConsoleTable("NAME", "MIN", "MID", "MAX", "COUNT");

            foreach (Probe probe in m_telemetry.GetProbes)
            {
                table.AddRow(probe.Name, $"{probe.MinValue.ToString("0.####")} ms", $"{probe.MidValue.ToString("0.####")} ms", $"{probe.MaxValue.ToString("0.####")} ms", probe.Count.ToString());
            }
            table.SortingBy("MID");
            return table;
        }


    }
}

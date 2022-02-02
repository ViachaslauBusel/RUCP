using RUCPs.Console;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUCPs.Logger

{
    public class TerminalLogger
        
    {
        private ConcurrentQueue<Note> m_messages { get; } = new ConcurrentQueue<Note>();
        public string Format { get; set; } = "[%date][%thread] - %message";
        internal event Action<Note> print;

        internal int MessagesCount => m_messages.Count;
        internal IEnumerable<Note> GetMessagesEnumerable() => m_messages;
       
        public TerminalLogger()
        {
            Terminal.AddCommand(new TerminalLoggerCommand(this));
        }

        /// <summary>
        /// Removes the specified number of messages starting from the first element
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        internal int RemoveFirstMessages(int count)
        {
            int deleted = 0;
            while (count-- > 0 && m_messages.TryDequeue(out Note e)) { deleted++; }
            return deleted;
        }

        public void Debug(string message)
        {
            Note _m = new Note(message, Environment.StackTrace, Thread.CurrentThread.Name, MsgType.Debug);
            m_messages.Enqueue(_m);
            print?.Invoke(_m);
        }

        public void Info(string message)
        {
            Note _m = new Note(message, Environment.StackTrace, Thread.CurrentThread.Name, MsgType.INFO);
            m_messages.Enqueue(_m);
            print?.Invoke(_m);
        }
        public void Warn(string message)
        {
            Note _m = new Note(message, Environment.StackTrace, Thread.CurrentThread.Name, MsgType.WARNING);
            m_messages.Enqueue(_m);
            print?.Invoke(_m);
        }
        public void Error(string message)
        {
            Note _m = new Note(message, Environment.StackTrace, Thread.CurrentThread.Name, MsgType.ERROR);
            m_messages.Enqueue(_m);
            print?.Invoke(_m);
        }
        public void Fatal(string message)
        {
            Note _m = new Note(message, Environment.StackTrace, Thread.CurrentThread.Name, MsgType.FATAL);
            m_messages.Enqueue(_m);
            print?.Invoke(_m);
        }

       
    }
}

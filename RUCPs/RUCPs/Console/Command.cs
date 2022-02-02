/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCPs.Console
{
    public class Command : ICommand
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        private Action<Queue<string>> action;

        public Command(string name, Action<Queue<string>> action) : this(name, "", action) { }
        public Command(string name, string description, Action<Queue<string>> action)
        {
            Name = name;
            Description = description;
            this.action = action;
        }

        void ICommand.Process(Queue<string> command)
        {
            action?.Invoke(command);
        }

    }
}

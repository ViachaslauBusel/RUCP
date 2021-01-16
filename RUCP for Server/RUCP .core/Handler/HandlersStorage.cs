/* BSD 3-Clause License
 *
 * Copyright (c) 2020, Vyacheslav Busel (yazZ3va)
 * All rights reserved. */

using System;
using System.Reflection;

namespace RUCP.Handler
{
    public class HandlersStorage<T> where T : Delegate
    {
        private  T[] handlers;

        public HandlersStorage(int capacity, bool attribute = true)
        {
            handlers = new T[capacity];
            if (attribute) Initial();
        }

        private void Initial()
        {
            foreach (Type @class in Assembly.GetEntryAssembly().GetTypes())
            {
                foreach (MethodInfo method in @class.GetMethods())
                {
                    foreach (Attribute attribute in method.GetCustomAttributes(typeof(HandlerAttribute), true))
                    {
                        if (!method.IsStatic) { System.Console.Error.WriteLine($"The '{method.Name}' method for handling messages must be static"); continue; }

                        HandlerAttribute handlerAttribute = attribute as HandlerAttribute;

                        if(handlerAttribute.Number < 0 || handlerAttribute.Number >= handlers.Length)
                        { System.Console.Error.WriteLine($"The '{method.Name}' method is assigned a type that is outside the Handlers Storage"); continue; }

                        try { handlers[handlerAttribute.Number] = method.CreateDelegate<T>(); }
                        catch (ArgumentException) { System.Console.Error.WriteLine($"The method '{method.Name}' has invalid parameters"); }
                    }
                }
            }
        }
        public void RegisterHandler(int type, T action)
        {
            if (type < 0 || type >= handlers.Length) return;

            handlers[type] = action;
        }

        public void UnregisterHandler(int type)
        {
            if (type < 0 || type >= handlers.Length) return;

            handlers[type] = null;
        }
        public T GetHandler(int type)
        {
            if (type < 0 || type >= handlers.Length) return null;
                return handlers[type];
        }
    }
}

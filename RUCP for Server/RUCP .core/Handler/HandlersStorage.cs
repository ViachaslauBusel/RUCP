using RUCP.Client;
using RUCP.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                        if (!method.IsStatic) { Console.Error.WriteLine($"The '{method.Name}' method for handling messages must be static"); continue; }

                        HandlerAttribute handlerAttribute = attribute as HandlerAttribute;

                        if(handlerAttribute.Number < 0 || handlerAttribute.Number >= handlers.Length)
                        { Console.Error.WriteLine($"The '{method.Name}' method is assigned a type that is outside the Handlers Storage"); continue; }

                        try { handlers[handlerAttribute.Number] = method.CreateDelegate<T>(); }
                        catch (ArgumentException) { Console.Error.WriteLine($"The method '{method.Name}' has invalid parameters"); }
                    }
                }
            }
        }
        public T GetHandler(int number)
        {
            return handlers[number];
        }
    }
}

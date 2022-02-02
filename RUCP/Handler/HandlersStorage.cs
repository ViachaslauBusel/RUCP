using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RUCP.Handler
{
    public class HandlersStorage<T> where T : Delegate
    {
        private Dictionary<int, T> m_handlers = new Dictionary<int, T>();
        //Метод для обработки неизвестных пакетов
        private T m_unknown;




        public void RegisterUnknown(T unknownType)
        {
            m_unknown = unknownType;
        }

        public void RegisterHandler(int type, T method)
        {
            if (m_handlers.ContainsKey(type)) m_handlers.Remove(type);
            m_handlers.Add(type, method);
        }

        public void UnregisterHandler(int type)
        {
            
           if(m_handlers.ContainsKey(type)) m_handlers.Remove(type);
        }

        public T GetHandler(int type)
        {

            if (!m_handlers.ContainsKey(type)) return m_unknown;
            return m_handlers[type];

        }

        public void RegisterAllStaticHandlers()
        {
            foreach (Type @class in Assembly.GetEntryAssembly().GetTypes())
            {
                foreach (MethodInfo method in @class.GetMethods())
                {
                    foreach (Attribute attribute in method.GetCustomAttributes(typeof(HandlerAttribute), true))
                    {
                        if (!method.IsStatic) { System.Console.Error.WriteLine($"The '{method.Name}' method for handling messages must be static"); continue; }

                        HandlerAttribute handlerAttribute = attribute as HandlerAttribute;


                        try { RegisterHandler(handlerAttribute.Number, (T)method.CreateDelegate(typeof(T))); }
                        catch (ArgumentException) { System.Console.Error.WriteLine($"The method '{method.Name}' has invalid parameters"); }
                    }
                }
            }
        }
    }
}

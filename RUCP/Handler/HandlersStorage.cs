using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RUCP.Handler
{
    public class HandlersStorage<T> where T : Delegate
    {
    private static Dictionary<int, T> m_dumpStorage = new Dictionary<int, T>();
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
            lock (m_dumpStorage)
            {
                if (m_dumpStorage.Count > 0)
                {
                    foreach (var handlerAttribute in m_dumpStorage)
                    {
                        try { RegisterHandler(handlerAttribute.Key, handlerAttribute.Value); }
                        catch (ArgumentException) { System.Console.Error.WriteLine($"The method '{handlerAttribute.Key}' has invalid parameters"); }
                    }
                }
                else
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (Type @class in assembly.GetTypes())
                        {
                            foreach (MethodInfo method in @class.GetMethods())
                            {
                                foreach (Attribute attribute in method.GetCustomAttributes(typeof(HandlerAttribute), true))
                                {
                                    if (!method.IsStatic) { System.Console.Error.WriteLine($"The '{method.Name}' method for handling messages must be static"); continue; }

                                    HandlerAttribute handlerAttribute = attribute as HandlerAttribute;


                                    try
                                    {
                                        m_dumpStorage.Add(handlerAttribute.Number, (T)method.CreateDelegate(typeof(T)));
                                        RegisterHandler(handlerAttribute.Number, (T)method.CreateDelegate(typeof(T)));
                                    }
                                    catch (ArgumentException) { System.Console.Error.WriteLine($"The method '{method.Name}' has invalid parameters"); }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

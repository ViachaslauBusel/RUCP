using System;
using System.Reflection;

namespace RUCP.Handler
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute : Attribute
    {
        public int Number { get; private set; }
        public MethodInfo Method { get; set; }

        public HandlerAttribute(int number)
        {
            Number = number;     
        }

        internal T CreateMethodT<T>() where T : Delegate
        {
            return (T)Method?.CreateDelegate(typeof(T));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RUCP.Handler
{
    public class SpecificHandlerStorage
    {
        private Dictionary<int, Type> m_resolvers = new Dictionary<int, Type>();
        public void RegisterResolver<R>(int OpCode) where R : struct
        {
            m_resolvers.Add(OpCode, typeof(R));
        }
        public void RegisterHandler<M>(int opCode, M method) where M : Delegate
        {
            RegisterHandler(opCode, m_resolvers[opCode]);
        }
        public unsafe void Process(Packet packet)
        {
          if(m_resolvers.ContainsKey(packet.OpCode))
          {
                Type type = m_resolvers[packet.OpCode];
                type obj = Marshal.PtrToStructure(packet.ReadIntPtr(), type);
          }
        }
    }
}

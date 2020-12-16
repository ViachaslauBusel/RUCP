using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUCP.Debugger
{
    public abstract class DebugObject
    {
        internal virtual void Log(string message) { }
        internal virtual void LogError(string className, string message, string stackTrace) { }
    }
}

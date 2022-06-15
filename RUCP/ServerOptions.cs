using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class ServerOptions
    {
        /// <summary>
        /// Server mode. 
        /// Manual - to process packets, you need to call the ProcessPacket method
        /// Automatic - requests are processed automatically
        /// </summary>
        public Mode Mode { get; set; } = Mode.Automatic;
        /// <summary>
        /// Maximum number of threads processing client requests
        /// </summary>
        public int MaxParallelism { get; set; } = 8;
        /// <summary>
        /// Waiting time before writing data to the socket for maximum filling of the MTU
        /// </summary>
        public int SendTimeout { get; set; } = 2;
        /// <summary>
        /// Time to wait for a response from the remote host before disconnecting it
        /// </summary>
        public int DisconnectTimeout { get; set; } = 6_000;

        internal ServerOptions Clone() => new ServerOptions()
        {
            Mode = this.Mode,
            MaxParallelism = this.MaxParallelism,
            SendTimeout = this.SendTimeout,
            DisconnectTimeout = this.DisconnectTimeout
        };
    }
}

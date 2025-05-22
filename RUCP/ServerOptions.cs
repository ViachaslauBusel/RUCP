using System;
using System.Collections.Generic;
using System.Text;

namespace RUCP
{
    public class ServerOptions : ClientOptions
    {
        /// <summary>
        /// Server mode. 
        /// Manual - to process packets, you need to call the ProcessPacket method
        /// Automatic - requests are processed automatically
        /// </summary>
        public ServerMode Mode { get; set; } = ServerMode.Automatic;
        /// <summary>
        /// Maximum number of threads processing client requests
        /// </summary>
        public int MaxParallelism { get; set; } = 8;
    

        internal ServerOptions Clone() => new ServerOptions()
        {
            Mode = this.Mode,
            MaxParallelism = this.MaxParallelism,
            SendTimeout = this.SendTimeout,
            DisconnectTimeout = this.DisconnectTimeout,
            SendBufferSize = this.SendBufferSize
        };
    }
}

using System;
using Proxy.Plugin;

namespace BEFilterMonitor
{
    public class BEFilterMonitorException : IPluginException
    {
        public BEFilterMonitorException()
        {
        }

        public BEFilterMonitorException(string message)
            : base(message)
        {
        }

        public BEFilterMonitorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

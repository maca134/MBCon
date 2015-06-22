using System;
using Proxy.Plugin;

namespace WebLogger
{
    public class WebLoggerException : IPluginException
    {
        public WebLoggerException()
        {
        }

        public WebLoggerException(string message)
            : base(message)
        {
        }

        public WebLoggerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

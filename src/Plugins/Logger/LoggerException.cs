using System;
using Proxy.Plugin;

namespace Logger
{
    public class LoggerException : IPluginException
    {
        public LoggerException()
        {
        }

        public LoggerException(string message)
            : base(message)
        {
        }

        public LoggerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

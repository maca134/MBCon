using System;

namespace Proxy.Plugin
{
    abstract public class IPluginException : Exception
    {
        protected IPluginException()
        {
        }

        protected IPluginException(string message)
            : base(message)
        {
        }

        protected IPluginException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

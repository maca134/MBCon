using System;
using Proxy.Plugin;

namespace RestartMessages
{
    public class RestartMessagesException : IPluginException
    {
        public RestartMessagesException()
        {
        }

        public RestartMessagesException(string message)
            : base(message)
        {
        }

        public RestartMessagesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

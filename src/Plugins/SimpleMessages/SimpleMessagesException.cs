using System;
using Proxy.Plugin;

namespace SimpleMessages
{
    public class SimpleMessagesException : IPluginException
    {
        public SimpleMessagesException()
        {
        }

        public SimpleMessagesException(string message)
            : base(message)
        {
        }

        public SimpleMessagesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

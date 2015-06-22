using System;
using Proxy.Plugin;

namespace WebRcon
{
    public class WebRconException : IPluginException
    {
        public WebRconException()
        {
        }

        public WebRconException(string message)
            : base(message)
        {
        }

        public WebRconException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

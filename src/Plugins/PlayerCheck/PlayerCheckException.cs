using System;
using Proxy.Plugin;

namespace PlayerCheck
{
    public class PlayerCheckException : IPluginException
    {
        public PlayerCheckException()
        {
        }

        public PlayerCheckException(string message)
            : base(message)
        {
        }

        public PlayerCheckException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

using System;
using Proxy.Plugin;

namespace PlayerCheck.Drivers
{
    class DriverException : IPluginException
    {
        public DriverException()
        {
        }

        public DriverException(string message)
            : base(message)
        {
        }

        public DriverException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

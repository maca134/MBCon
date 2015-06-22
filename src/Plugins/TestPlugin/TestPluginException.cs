using System;
using Proxy.Plugin;

namespace TestPlugin
{
    public class TestPluginException : IPluginException
    {
        public TestPluginException()
        {
        }

        public TestPluginException(string message)
            : base(message)
        {
        }

        public TestPluginException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

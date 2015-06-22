using System;

namespace Proxy.BE
{
    public class CommandException : Exception
    {
        public CommandException()
        {
        }

        public CommandException(string message)
            : base(message)
        {
        }

        public CommandException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

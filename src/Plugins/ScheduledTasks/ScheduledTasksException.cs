using System;
using Proxy.Plugin;

namespace ScheduledTasks
{
    public class ScheduledTasksException : IPluginException
    {
        public ScheduledTasksException()
        {
        }

        public ScheduledTasksException(string message)
            : base(message)
        {
        }

        public ScheduledTasksException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

using System;

namespace MBCon.classes
{
    public class SettingsException : Exception
    {
        public SettingsException()
        {
        }

        public SettingsException(string message)
            : base(message)
        {
        }

        public SettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

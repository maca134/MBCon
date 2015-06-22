using System;
using System.IO;

namespace Proxy
{
    public class AppConsole
    {
        private static StreamWriter _log;

        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (_log == null)
            {
                try
                {
                    _log = new StreamWriter("output.log", true) {AutoFlush = true};
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not open output log: {0}", ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    _log = StreamWriter.Null;
                }
            }
            _log.WriteLine(message);
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

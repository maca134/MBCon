using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MBCon.classes;
using Proxy;

namespace MBCon
{
    class Program
    {
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                core.Kill();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static Core core;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate
            {
                core.Kill();
            }; 
            handler = ConsoleEventCallback;
            SetConsoleCtrlHandler(handler, true);

            Console.Title = "MBCon - Connected";
            Console.OutputEncoding = Encoding.UTF8;
            core = new Core();
            try
            {
                core.Run(args);
            }
            catch (CoreException ex)
            {
                core.Kill();
                AppConsole.Log(String.Format("ERROR: {0}", ex.Message), ConsoleColor.Red);
                Thread.Sleep(5000);
            }
        }

    }
}

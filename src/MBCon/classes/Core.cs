using System;
using System.IO;
using System.Reflection;
using System.Threading;
using BattleNET;
using Fclp;
using Proxy;

namespace MBCon.classes
{
    public class Core
    {
        public static string BasePath
        {
            get
            {
                var directoryInfo = (new FileInfo(Assembly.GetEntryAssembly().Location)).Directory;
                return directoryInfo != null ? directoryInfo.ToString() : null;
            }
        }

        public string PluginPath
        {
            get
            {
                return _settings.PluginPath;
            }
        }

        private readonly FluentCommandLineParser _args = new FluentCommandLineParser();
        private bool _connected;
        private string _configPath = "";
        private int _startupDelay;
        private Settings _settings;
        private BattlEyeClient _beclient;
        private Api _api;
        private PluginManager _pluginManager;
        private const int _maxtries = 5;

        public void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.Title = "MBCon - Connecting...";
#if DEBUG
            AppConsole.Log(String.Format("MBCon - WARNING THIS IS A DEBUG APP!!!"), ConsoleColor.Red);
            AppConsole.Log(String.Format("MBCon - WARNING THIS IS A DEBUG APP!!!"), ConsoleColor.Red);
            AppConsole.Log(String.Format("MBCon - WARNING THIS IS A DEBUG APP!!!"), ConsoleColor.Red);
            AppConsole.Log(String.Format("MBCon - WARNING THIS IS A DEBUG APP!!!"), ConsoleColor.Red);
            AppConsole.Log(String.Format("MBCon - WARNING THIS IS A DEBUG APP!!!"), ConsoleColor.Red);
#else
            AppConsole.Log(String.Format("=========================="), ConsoleColor.DarkCyan);
            AppConsole.Log(String.Format("MBCon - by maca134"), ConsoleColor.Cyan);
            AppConsole.Log(String.Format("maca134@googlemail.com"), ConsoleColor.Gray);
            AppConsole.Log(String.Format("=========================="), ConsoleColor.DarkCyan);
            AppConsole.Log("");
            Thread.Sleep(4000);
#endif

            _args.Setup<string>("config")
                .Callback(val => _configPath = val)
                .SetDefault(Path.Combine(BasePath, "config.ini"));

            _args.Setup<int>("delay")
                .Callback(val => _startupDelay = val)
                .SetDefault(0);

            _args.Parse(args);

            if (!File.Exists(_configPath))
            {
                throw new CoreException(String.Format("Config file \"{0}\" was not found.", _configPath));
            }
            AppConsole.Log("Config file found, continuing to load...");
            var ini = new IniParser(_configPath);

            try
            {
                _settings = new Settings(ini);
            }
            catch (SettingsException ex)
            {
                throw new CoreException(String.Format("Error Loading Settings: {0}", ex.Message));
            }

            if (_startupDelay > 0)
            {
                AppConsole.Log(string.Format("Waiting for {0} seconds", _startupDelay));
                Thread.Sleep(_startupDelay * 1000);
            }

            _beclient = BEClient();
            _pluginManager = new PluginManager(PluginPath);

            _api = new Api(_beclient, _settings);
            _pluginManager.Init(_api);

            AppConsole.Log("Connecting to server...");

            Connect();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            AppConsole.Log(String.Format("Unhandled Exception: {0} - {1}", e.Message, e.StackTrace));
        }

        public void Connect()
        {
            for (var ia = 1; ia <= 3; ia++)
            {
                for (var tries = 1; tries <= _maxtries; tries++)
                {
                    try
                    {
                        _beclient.Connect();
                        if (!_beclient.Connected)
                            throw new CoreException();
                        break;
                    }
                    catch
                    {
                        AppConsole.Log(String.Format("Failed to connect to server. Attempt {0}/{1}", tries, _maxtries), ConsoleColor.Red);
                    }
                    Thread.Sleep(1000);
                }
                Thread.Sleep(60000);
            }

            if (!_beclient.Connected)
            {
                throw new CoreException("Failed to connect to server.");
            }
            _connected = true;
        }

        private BattlEyeClient BEClient()
        {
            AppConsole.Log("Loading BEClient...");
            var login = new BattlEyeLoginCredentials
            {
                Host = _settings.Address,
                Port = _settings.Port,
                Password = _settings.RconPass
            };

            var beclient = new BattlEyeClient(login) {ReconnectOnPacketLoss = true};
            beclient.BattlEyeConnected += BattlEyeConnected;
            beclient.BattlEyeDisconnected += BattlEyeDisconnected;
            return beclient;
        }

        private void BattlEyeDisconnected(BattlEyeDisconnectEventArgs args)
        {
            AppConsole.Log("BE Disconnected", ConsoleColor.Yellow);
            Thread.Sleep(2000);
            Environment.Exit(0);
        }

        private void BattlEyeConnected(BattlEyeConnectEventArgs args)
        {
            if (args.ConnectionResult == BattlEyeConnectionResult.Success)
            {
                AppConsole.Log(String.Format("Connected to {0}:{1}", _settings.Address, _settings.Port));
            }
            else
            {
                AppConsole.Log(String.Format("Connection to {0}:{1} failed: {2}", _settings.Address, _settings.Port, args.Message), ConsoleColor.Red);
                if (_connected)
                {
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
            }
        }

        internal void Kill()
        {
            AppConsole.Log("Unloading MBCon");
            try
            {
                _pluginManager.Kill();
                _beclient.Disconnect();
                _api.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}

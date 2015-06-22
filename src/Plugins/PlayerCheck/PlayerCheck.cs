using System;
using System.IO;
using PlayerCheck.Drivers;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace PlayerCheck
{
    public class PlayerCheck : PluginBase, IPlugin
    {
        private enum Mode
        {
            Blacklist,
            Whitelist
        }

        public string Name
        {
            get
            {
                return "PlayerCheck";
            }
        }

        public string Author
        {
            get
            {
                return "maca134";
            }
        }

        private string _configPath
        {
            get
            {
                return Path.Combine(new[] { _dllpath, "config.ini" });
            }
        }

        private bool _enabled = true;
        private Mode _mode = Mode.Blacklist;
        private string _kickMessage = "";
        private bool _checkip;
        private string _dllpath;
        private IDriver _driver;
        private IApi _api;

        public void Init(IApi api, string dllpath)
        {
            _api = api;
            _dllpath = dllpath;
            IniParser _ini;
            try
            {
                _ini = new IniParser(_configPath);
            }
            catch (Exception ex)
            {
                throw new PlayerCheckException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new PlayerCheckException(String.Format("{0} has been disabled", Name));

            _checkip = _ini.GetBoolSetting(Name, "CheckIP");
            _kickMessage = _ini.GetSetting(Name, "KickMessage");

            if (_ini.GetSetting(Name, "Mode") == "white")
            {
                _mode = Mode.Whitelist;
            }

            var settings = new DriverSettings()
            {
                Api = api,
                Ini = _ini,
                PluginPath = dllpath
            };

            _driver = Base.GetDriver(_ini);
            _driver.SetConfig(settings);
            api.OnBeMessageReceivedEvent += onBEMessageReceivedEvent;
        }

        void onBEMessageReceivedEvent(Message message)
        {
            if (!(
                message.Type == Message.MessageType.ConnectLegacyGUID ||
                message.Type == Message.MessageType.ConnectGUID ||
                (_checkip && message.Type == Message.MessageType.ConnectIP)
                ))
                return;
            Player player;

            try
            {
                player = new Player(message);
            }
            catch (Exception ex)
            {
                AppConsole.Log(String.Format("Error paring be message: {0}", ex.Message), ConsoleColor.Red);
                return;
            }

            bool check;
            try
            {
                check = _driver.CheckPlayer(player);
            }
            catch (Exception ex)
            {
                AppConsole.Log(String.Format("Error checking player: {0}", ex.Message), ConsoleColor.Red);
                Console.WriteLine(ex.StackTrace);
                return;
            }

            var kick = (check && _mode == Mode.Blacklist) || (!check && _mode == Mode.Whitelist);

            if (kick)
            {
                var cmd = new Command()
                {
                    Type = Command.CmdType.Kick,
                    Parameters = String.Format("{0} {1}", player.Id, _kickMessage)
                };
                AppConsole.Log(String.Format("Kicking {0}", player.Name));
                _api.SendCommand(cmd);
            }

        }

        public void Kill()
        {

        }
    }
}

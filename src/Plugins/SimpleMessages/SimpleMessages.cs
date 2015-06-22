using System;
using System.IO;
using System.Linq;
using System.Timers;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace SimpleMessages
{
    public class SimpleMessages : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "SimpleMessages";
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

        private IApi _api;
        private IniParser _ini;
        private bool _enabled = true;
        string[] _messages;
        int _messagePointer;
        bool _repeat = true;
        string _prefix = "";
        Timer _timer;
        private string _dllpath;

        public void Init(IApi api, string dllpath)
        {
            _api = api;
            _dllpath = dllpath;
            try
            {
                _ini = new IniParser(_configPath);
            }
            catch (Exception ex)
            {
                throw new SimpleMessagesException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting("SimpleMessages", "Enabled");
            if (!_enabled)
                throw new SimpleMessagesException(String.Format("{0} has been disabled", Name));

            Int32 _interval;
            try
            {
                _interval = Convert.ToInt32(_ini.GetSetting("SimpleMessages", "Interval", "60"));
            }
            catch
            {
                _interval = 60;
            }

            _prefix = _ini.GetSetting("SimpleMessages", "Prefix");

            _repeat = _ini.GetBoolSetting("SimpleMessages", "Repeat");
            var _random = _ini.GetBoolSetting("SimpleMessages", "Random");
            _messages = _ini.EnumSection("Messages");
            if (_random)
            {
                var rnd = new Random();
                _messages = _messages.OrderBy(x => rnd.Next()).ToArray();
            }
            _timer = new Timer {Interval = (_interval*1000)};
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var message = _ini.GetSetting("Messages", _messages[_messagePointer]);
            _messagePointer++;
            if (String.IsNullOrWhiteSpace(message))
            {
                return;
            }
            message = String.Format("{0} {1}", _prefix, message);
            AppConsole.Log(String.Format("{0}: {1}", Name, message), ConsoleColor.Magenta);

            var cmd = new Command() { 
                Type = Command.CmdType.Say, 
                Parameters = String.Format("-1 {0}", message) 
            };
            _api.SendCommand(cmd);

            if (_messagePointer >= _messages.Length)
            {
                if (_repeat)
                {
                    _messagePointer = 0;
                }
                else
                {
                    Kill();
                }
            }
        }

        public void Kill()
        {
            try
            {
                _timer.Enabled = false;
                _timer.Dispose();
                _timer.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}

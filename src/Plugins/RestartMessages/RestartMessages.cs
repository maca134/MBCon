using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Humanizer;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace RestartMessages
{
    public class RestartMessages : PluginBase, IPlugin, IDisposable
    {
        public string Name
        {
            get
            {
                return "RestartMessages";
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

        private Thread _worker;
        private volatile bool _workerTerminateSignal;
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private IniParser _ini;
        private bool _enabled = true;
        private IApi _api;
        private string _dllpath;
        private bool _debug;

        private DateTime _runtime;
        private readonly List<DateTime> _intervals = new List<DateTime>();
        private int _repeat = 1;
        private String _message = "";


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
                throw new RestartMessagesException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new RestartMessagesException(String.Format("{0} has been disabled", Name));

            _debug = _ini.GetBoolSetting("RestartMessages", "Debug");

            try
            {
                _runtime = DateTime.Now.AddHours(Convert.ToInt16(_ini.GetSetting("RestartMessages", "ServerRuntime").Trim()));
            }
            catch (Exception ex)
            {
                throw new RestartMessagesException(String.Format("Could not parse ServerRuntime: {0}", ex.Message));
            }

            try
            {
                var intervals = _ini.GetSetting("RestartMessages", "Interval");
                foreach (var i in intervals.Split(','))
                {
                    _intervals.Add(_runtime.AddMinutes(-1 * Convert.ToInt16(i)));
                }
                _intervals.Sort();
                if (_intervals.Count == 0)
                {
                    throw new RestartMessagesException("Not intervals have been set");
                }
            }
            catch (Exception ex)
            {
                throw new RestartMessagesException(String.Format("Could not parse Interval: {0}", ex.Message));
            }

            try
            {
                _repeat = Convert.ToInt16(_ini.GetSetting("RestartMessages", "Repeat").Trim());
            }
            catch (Exception ex)
            {
                throw new RestartMessagesException(String.Format("Could not parse Repeat: {0}", ex.Message));
            }

            try
            {
                _message = _ini.GetSetting("RestartMessages", "Message").Trim();
            }
            catch (Exception ex)
            {
                throw new RestartMessagesException(String.Format("Could not parse Message: {0}", ex.Message));
            }

            _worker = new Thread(TaskMonitor);
            _worker.Start();
        }

        private void TaskMonitor()
        {
            var messagePointer = 0;
            while (!_workerTerminateSignal)
            {
                var next = _intervals[messagePointer];

                messagePointer++;
                var ts = next - DateTime.Now;
                if (ts.TotalSeconds > 0)
                {
                    _waitHandle.WaitOne(ts);
                    if (_workerTerminateSignal)
                        break;
                }
                var restart = _runtime - DateTime.Now;
                var message = String.Format(_message, restart.Humanize(2));
                for (var i = 1; i <= _repeat; i++)
                {
                    AppConsole.Log(String.Format("Sending Message: {0}", message), ConsoleColor.DarkMagenta);
                    if (!_debug)
                    {
                        _api.SendCommand(new Command(String.Format("say -1 {0}", message)));
                    }
                }
            }
        }

        public void Dispose()
        {
            Kill();
        }

        public void Kill()
        {
            if (_worker != null)
            {
                _workerTerminateSignal = true;
                _waitHandle.Set();
                if (!_worker.Join(TimeSpan.FromMinutes(1)))
                {
                    AppConsole.Log("Worker busy, aborting the thread.", ConsoleColor.Red);
                    _worker.Abort();
                }
                _worker = null;
            }
        }
    }
}

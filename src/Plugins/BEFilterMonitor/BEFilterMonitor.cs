using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace BEFilterMonitor
{
    public class BEFilterMonitor : PluginBase, IPlugin, IDisposable
    {
        public string Name
        {
            get
            {
                return "BEFilterMonitor";
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
        private const int _interval = 4000;

        private readonly string[] _filters = new[] { 
            "addbackpackcargo.txt",
            "addmagazinecargo.txt",
            "addweaponcargo.txt",
            "attachto.txt",
            "createvehicle.txt",
            "deleteVehicle.txt",
            "mpeventhandler.txt",
            "publicvariable.txt",
            "publicvariableval.txt",
            "remotecontrol.txt",
            "remoteexec.txt",
            "scripts.txt",
            "selectplayer.txt",
            "setdamage.txt",
            "setpos.txt",
            "setvariable.txt",
            "setvariableval.txt",
            "teamswitch.txt",
            "waypointcondition.txt",
            "waypointstatement.txt"
        };
        private Dictionary<string, DateTime> _modified;

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
                throw new BEFilterMonitorException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new BEFilterMonitorException(String.Format("{0} has been disabled", Name));


            _worker = new Thread(FilterMonitor);
            _worker.Start();
        }

        private void FilterMonitor()
        {
            _modified = new Dictionary<string, DateTime>();
            foreach (var f in _filters)
            {
                var file = Path.Combine(_api.Settings.BePath, f);
                if (!File.Exists(file))
                {
                    AppConsole.Log(String.Format("Could not find BEFilter: {0}", f));
                    continue;
                }
                var info = new FileInfo(file);
                _modified.Add(file, info.LastWriteTime);
            }

            while (!_workerTerminateSignal)
            {
                _waitHandle.WaitOne(_interval);
                if (_workerTerminateSignal)
                    break;

                foreach (var key in new List<string>(_modified.Keys))
                {
                    var info = new FileInfo(key);
                    if (info.LastWriteTime > _modified[key])
                    {
                        _modified[key] = info.LastWriteTime;

                        AppConsole.Log(String.Format("Filter {0} has been updated.", info.Name));
                        Command command;
                        if (info.Name == "scripts")
                        {
                            command = new Command()
                            {
                                Type = Command.CmdType.LoadScripts
                            };
                        }
                        else
                        {
                            command = new Command()
                            {
                                Type = Command.CmdType.loadEvents
                            };
                        }
                        _api.SendCommand(command);
                    }
                }
            }
        }

        public void Dispose()
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
            _waitHandle.Dispose();
        }

        public void Kill()
        {
            Dispose();
        }
    }
}

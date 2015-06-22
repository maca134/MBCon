using System;
using System.IO;
using System.Threading;
using Humanizer;
using Proxy;
using Proxy.Plugin;

namespace ScheduledTasks
{
    public class ScheduledTasks : PluginBase, IPlugin, IDisposable
    {
        public string Name
        {
            get
            {
                return "ScheduledTasks";
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
        private bool _debug;
        private string _schedule = "";
        private IApi _api;
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
                throw new ScheduledTasksException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting("ScheduledTasks", "Enabled");
            if (!_enabled)
                throw new ScheduledTasksException(String.Format("{0} has been disabled", Name));

            _debug = _ini.GetBoolSetting("ScheduledTasks", "Debug");
            _schedule = _ini.GetSetting("ScheduledTasks", "Schedule");
            if (_schedule == "")
                throw new ScheduledTasksException(String.Format("{0} has been disabled. No schedule defined.", Name));

            if (!Path.IsPathRooted(_schedule))
            {
                _schedule = Path.Combine(_dllpath, _schedule);
            }

            var ext = Path.GetExtension(_schedule);
            if (ext == ".json")
            {
                Task.LoadFromJSON(_schedule);
            }
            
            if (_debug)
            {
                var i = 0;
                AppConsole.Log("---------------", ConsoleColor.DarkMagenta);
                AppConsole.Log("Scheduled Tasks", ConsoleColor.DarkMagenta);
                AppConsole.Log("---------------", ConsoleColor.DarkMagenta);

                foreach (var t in Task.Tasks)
                {
                    i++;
                    AppConsole.Log(String.Format("Task {0}", i), ConsoleColor.DarkMagenta);
                    AppConsole.Log(String.Format("Running in about {0}", t.TimeSpan.Humanize(2, true)), ConsoleColor.DarkMagenta);
                    if (t.Loop != 1)
                    {
                        AppConsole.Log(String.Format("Repeating every {0}, {1} times", t.Interval.Humanize(2, true), t.Loop), ConsoleColor.DarkMagenta);
                    }
                    AppConsole.Log(String.Format("Command: {0} {1}", t.Command.Type, t.Command.Parameters), ConsoleColor.DarkMagenta);
                    AppConsole.Log("");
                }
            }
            if (Task.Tasks.Count == 0)
            {
                throw new ScheduledTasksException(String.Format("{0} has been disabled. No tasks found.", Name));
            }

            _worker = new Thread(TaskMonitor);
            _worker.Start();
        }

        private void TaskMonitor()
        {
            while (!_workerTerminateSignal)
            {
                Task t;
                lock (Task.Tasks)
                {
                    t = Task.Next;
                }
                _waitHandle.WaitOne(t.TimeSpan);
                if (_workerTerminateSignal)
                    break;
                if (_debug)
                {
                    AppConsole.Log(String.Format("Running Task: {0} {1}", t.Command.Type, t.Command.Parameters), ConsoleColor.DarkMagenta);
                }
                else
                {
                    _api.SendCommand(t.Command);
                }
                lock (Task.Tasks)
                {
                    t.Increment();
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

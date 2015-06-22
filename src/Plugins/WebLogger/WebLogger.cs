using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace WebLogger
{
    public struct Job
    {
        public string type;
        public string message;

        public static implicit operator Job(Message message)
        {
            return new Job() { type = message.Type.ToString(), message = message.Content };
        }
    }

    public class WebLogger : PluginBase, IPlugin, IDisposable
    {
        private Thread _worker;
        private volatile bool _workerTerminateSignal;
        private readonly Queue<Job> _processingQueue = new Queue<Job>();
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private bool _hasQueuedItem
        {
            get
            {
                lock (_processingQueue)
                {
                    return _processingQueue.Any();
                }
            }
        }

        private Job _nextQueuedItem
        {
            get
            {
                lock (_processingQueue)
                {
                    return _processingQueue.Dequeue();
                }
            }
        }


        public string Name
        {
            get
            {
                return "WebLogger";
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
        private Uri _uri;
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
                throw new WebLoggerException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting("WebLogger", "Enabled");
            if (!_enabled)
                throw new WebLoggerException(String.Format("{0} has been disabled", Name));

            var url = _ini.GetSetting("WebLogger", "URL");
            if (!Uri.TryCreate(url, UriKind.Absolute, out _uri))
            {
                throw new WebLoggerException(String.Format("Error parsing url: {0}", url));
            }

            _worker = new Thread(ProcessQueue);
            _worker.Start();
            _api.OnBeMessageReceivedEvent += MessageEventHandler;
        }

        void MessageEventHandler(Message message)
        {
            Job job = message;
            lock (_processingQueue)
            {
                _processingQueue.Enqueue(job);
            }
            _waitHandle.Set();
        }

        private void ProcessQueue()
        {
            while (!_workerTerminateSignal)
            {
                if (!_hasQueuedItem)
                {
                    _waitHandle.WaitOne();
                }
                else
                {
                    try
                    {
                        SendLog(_nextQueuedItem);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void SendLog(Job job)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var reqparm = new NameValueCollection {{"message", job.message}, {"type", job.type}};
                    client.UploadValues(_uri, "POST", reqparm);
                }
            }
            catch (Exception ex)
            {
                AppConsole.Log(String.Format("Error posting log: {0}", ex.Message), ConsoleColor.Red);
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Proxy;
using Proxy.BE;
using Proxy.Plugin;

namespace Logger
{
    public class Logger : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "Logger";
            }
        }

        public string Author
        {
            get
            {
                return "maca134";
            }
        }

        private IApi _api;
        private IniParser _ini;
        private bool _enabled = true;
        private string _path;
        private readonly string _timestamp = DateTime.Now.ToString("yyyy-MM-dd");
        private string _dllpath;

        private readonly Dictionary<Message.MessageType, string> _logTypes = new Dictionary<Message.MessageType, string>() { 
            {Message.MessageType.Chat, "chat-{0}.log"},
            {Message.MessageType.ConnectGUID, "connect-{0}.log"},
            {Message.MessageType.ConnectIP, "connect-{0}.log"},
            {Message.MessageType.ConnectLegacyGUID, "connect-{0}.log"},
            {Message.MessageType.Disconnect, "connect-{0}.log"},
            {Message.MessageType.Kick, "kick-{0}.log"},
            {Message.MessageType.Log, "log-{0}.log"}
        };

        private readonly Dictionary<Message.MessageType, StreamWriter> _logFS = new Dictionary<Message.MessageType, StreamWriter>();

        private string _configPath
        {
            get
            {
                return Path.Combine(new[] { _dllpath, "config.ini" });
            }
        }

        private string _defaultLogPath
        {
            get
            {
                return Path.Combine(new[] { _dllpath, "logs" });
            }
        }

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
                throw new LoggerException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting("Logger", "Enabled");
            if (!_enabled)
            {
                throw new LoggerException(String.Format("{0} has been disabled", Name));
            }
            _path = _ini.GetSetting("Logger", "Path");
            if (_path == "")
                _path = _defaultLogPath;

            if (!Directory.Exists(_path))
            {
                try
                {
                    Directory.CreateDirectory(_path);
                }
                catch
                {
                    throw new LoggerException(String.Format("Log path \"{0}\" does not exist.", _path));
                }
            }

            OpenLogs();
            _api.OnBeMessageReceivedEvent += _be_MessageEventHandler;
        }

        void _be_MessageEventHandler(Message message)
        {
            var s = _logFS[message.Type];
            s.WriteLine("{0} {1}", message.Received.ToString("HH:mm:ss"), message.Content);
        }

        private void OpenLogs()
        {
            var streamWriter = new Dictionary<string, StreamWriter>();
            foreach (var lt in _logTypes)
            {
                var file = Path.Combine(_path, String.Format(lt.Value, _timestamp));
                if (!streamWriter.ContainsKey(file))
                {
                    try
                    {
                        streamWriter[file] = new StreamWriter(file, true) {AutoFlush = true};
                    }
                    catch (Exception ex)
                    {
                        throw new LoggerException(String.Format("Could not open {0}: {1}", file, ex.Message));
                    }
                }
                _logFS.Add(lt.Key, streamWriter[file]);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void Kill()
        {
            foreach (var lt in _logFS)
            {
                try
                {
                    var s = lt.Value;
                    s.Flush();
                    s.Close();
                    s.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}

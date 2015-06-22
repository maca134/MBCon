using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Proxy;
using Proxy.Plugin;
using WebSocketSharp.Server;

namespace WebRcon
{

    public class WebRcon : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "WebRcon";
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
        private WebServer _webServer;
        private WebSocketServer _socketServer;
        private int _port;
        private string _dllpath;
        private readonly Dictionary<string, string> _htdocs = new Dictionary<string, string>();
        private string _serverurl;
        private string _password;

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
                throw new WebRconException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new WebRconException(String.Format("{0} has been disabled", Name));

            var port = _ini.GetSetting(Name, "Port");
            try
            {
                _port = Convert.ToInt32(port);
            }
            catch (Exception ex)
            {
                throw new WebRconException(String.Format("Invalid port: {0}", ex.Message));
            }

            _password = _ini.GetSetting(Name, "Password");
            if (_password == "")
            {
                _password = Utils.GetRandomString();
            }

#if DEBUG
            _serverurl = String.Format("http://localhost:{0}/", _port);
#else
            _serverurl = String.Format("http://*:{0}/", _port);
#endif
            _webServer = new WebServer(HttpRequest, _serverurl);
            _webServer.Run();
            AppConsole.Log(String.Format("Started HTTP server at {0}. Password: {1}", _serverurl, _password), ConsoleColor.Cyan);

            _port++;
            _socketServer = new WebSocketServer(_port);
            _socketServer.AddWebSocketService("/rcon", () => new SocketBehavior(_api, _password));
            _socketServer.Start();

            LoadHtdocsFiles();

        }

        private HttpResponse HttpRequest(HttpListenerRequest arg)
        {
            var path = arg.Url.LocalPath.TrimStart('/');
            if (String.IsNullOrWhiteSpace(path))
            {
                path = "client.html";
            }
            HttpResponse response;
            try
            {
                response = new HttpResponse()
                {
                    Content = _htdocs[path],
                    Mime = MimeTypeMap.GetMimeType(Path.GetExtension(path))
                };
            }
            catch
            {
                throw new HttpFileNotFoundException();
            }
            return response;
        }

        private void LoadHtdocsFiles()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (!name.StartsWith("WebRcon.htdocs."))
                    continue;
                var str = assembly.GetManifestResourceStream(name);
                if (str != null)
                {
                    var sr = new StreamReader(str, Encoding.ASCII);

                    var key = name.Replace("WebRcon.htdocs.", "");
                    _htdocs.Add(key.Replace('/', '.'), sr.ReadToEnd());
                }
            }
        }

        public void Kill()
        {
            try
            {
                _webServer.Stop();
                _socketServer.Stop();
            }
            catch
            {
                // ignored
            }
        }

    }
}

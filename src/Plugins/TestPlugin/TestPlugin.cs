using System;
using System.IO;
using Proxy;
using Proxy.Plugin;

namespace TestPlugin
{
    public class TestPlugin : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "TestPlugin";
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

        private IniParser _ini;
        private bool _enabled = true;
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
                throw new TestPluginException(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new TestPluginException(String.Format("{0} has been disabled", Name));

        }

        public void Kill()
        {
            
        }
    }
}

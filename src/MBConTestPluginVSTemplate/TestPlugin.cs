using Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    public class $safeprojectname$ : PluginBase, IPlugin
    {
        public string Name
        {
            get
            {
                return "$safeprojectname$";
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
                return Path.Combine(new string[] { _dllpath, "config.ini" });
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
                throw new $safeprojectname$Exception(String.Format("Error loading config: {0}", ex.Message));
            }

            _enabled = _ini.GetBoolSetting(Name, "Enabled");
            if (!_enabled)
                throw new $safeprojectname$Exception(String.Format("{0} has been disabled", Name));
            
        }

        public void Kill()
        {
            
        }
    }
}

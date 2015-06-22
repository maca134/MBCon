using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Proxy;

namespace MBCon.classes
{
    public class Settings : ISettings
    {
        public static Settings Instance { get; private set; }

        public static void Forge(IniParser ini)
        {
            Instance = new Settings(ini);
        }

        private readonly IPAddress _address;
        private readonly Int32 _port;
        private readonly string _bepath;
        private readonly string _rconpass;
        private readonly string _pluginpath;
        private readonly Dictionary<string, string> _admins;

        public IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        public Int32 Port
        {
            get
            {
                return _port;
            }
        }

        public string RconPass
        {
            get
            {
                return _rconpass;
            }
        }

        public string PluginPath
        {
            get
            {
                return _pluginpath;
            }
        }

        public string BePath
        {
            get
            {
                return _bepath;
            }
        }

        public Dictionary<string, string> Admins
        {
            get
            {
                return _admins;
            }
        }

        public Settings(IniParser ini)
        {
            string beconfig;

            if (!IPAddress.TryParse(ini.GetSetting("General", "IP").Trim(), out _address))
            {
                throw new SettingsException("Port is not set.");
            }

            try
            {
                _port = Convert.ToInt32(ini.GetSetting("General", "Port").Trim());
                if (_port > 65535)
                    throw new SettingsException("Port is too high.");
            }
            catch (Exception ex)
            {
                throw new SettingsException(String.Format("Port is not set or invalid: {0}", ex.Message));
            }

            try
            {
                _bepath = ini.GetSetting("General", "BEPath").Trim();
                if (!Directory.Exists(_bepath))
                    throw new SettingsException("BEPath does not exist.");
                beconfig = Path.Combine(_bepath, "BEServer.cfg");
                if (!File.Exists(beconfig))
                {
                    var files = Directory.GetFiles(_bepath, "*.cfg")
                     .Where(path => path.Contains("BEServer_active_"))
                     .ToList();
                    if (files.Count == 0)
                        throw new SettingsException("Cound not find BEServer.cfg.");
                    beconfig = Path.Combine(_bepath, files[0]);
                }
            }
            catch (Exception ex)
            {
                throw new SettingsException(String.Format("BEPath is invalid: {0}", ex.Message));
            }

            try
            {
                var file = new StreamReader(beconfig);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith("RConPassword "))
                    {
                        _rconpass = line.Replace("RConPassword ", "");
                        break;
                    }
                }
                if (_rconpass == null)
                    throw new SettingsException("Could not find rcon password in BEServer.cfg.");
            }
            catch (Exception ex)
            {
                throw new SettingsException(String.Format("Error reading BEServer.cfg: {0}", ex.Message));
            }

            try
            {
                _pluginpath = ini.GetSetting("General", "PluginPath").Trim();
                if (_pluginpath == "")
                {
                    _pluginpath = Path.Combine(Core.BasePath, "plugins");
                }
                if (!Directory.Exists(_pluginpath))
                    Directory.CreateDirectory(_pluginpath);
            }
            catch (Exception ex)
            {
                throw new SettingsException(String.Format("Error reading plugin directory: {0}", ex.Message));
            }

            try
            {
                _admins = new Dictionary<string, string>();
                var adminsList = ini.EnumSection("Admins");
                foreach (var guid in adminsList)
                {
                    var name = ini.GetSetting("Admins", guid);
                    _admins.Add(guid.Trim(), name.Trim());
                }
            }
            catch (Exception ex)
            {
                throw new SettingsException(String.Format("Error reading settings: {0}", ex.Message));
            }

        }
    }
}

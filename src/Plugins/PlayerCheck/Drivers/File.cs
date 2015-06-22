using System;
using System.Collections.Generic;
using System.IO;
using Proxy;

namespace PlayerCheck.Drivers
{
    public class File : Base, IDriver
    {
        private readonly List<string> _list = new List<string>();

        new public void SetConfig(DriverSettings settings)
        {
            base.SetConfig(settings);
            var f = settings.Ini.GetSetting("File", "Path");
            if (!Path.IsPathRooted(f))
            {
                f = Path.Combine(settings.PluginPath, f);
            }
            if (!System.IO.File.Exists(f))
            {
                throw new DriverException(String.Format("Could not load player list {0}", f));
            }
            var file = new StreamReader(f);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                _list.Add(line.Trim());
            }
            AppConsole.Log(String.Format("PlayerCheck: Loaded {0} GUIDs into list.", _list.Count));
            file.Close();
        }
        
        public bool CheckPlayer(Player player)
        {
            return _list.Contains(player.Data);
        }
    }
}

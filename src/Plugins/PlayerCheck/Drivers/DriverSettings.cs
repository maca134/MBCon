using Proxy;

namespace PlayerCheck.Drivers
{
    public class DriverSettings
    {
        public IniParser Ini { get; set; }
        public IApi Api { get; set; }
        public string PluginPath { get; set; }
        public bool CheckIp { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Net;

namespace Proxy
{
    public interface ISettings
    {
        IPAddress Address { get; }
        Int32 Port { get; }
        string PluginPath { get; }
        string BePath { get; }
        Dictionary<string, string> Admins { get; }
    }
}

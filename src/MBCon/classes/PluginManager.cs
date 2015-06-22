using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Proxy;
using Proxy.Plugin;

namespace MBCon.classes
{
    public struct Plugin
    {
        public IPlugin Instance;
        public string DllPath;
    }

    public class PluginManager
    {
        private readonly List<Plugin> _pluginCollection = new List<Plugin>();

        public PluginManager(string pluginPath)
        {
            AppConsole.Log("Loading Plugins...");
            var dirs = Directory.GetDirectories(pluginPath, "*", SearchOption.TopDirectoryOnly);
            var pluginType = typeof(IPlugin);
            foreach (var dir in dirs)
            {
                var name = Path.GetFileName(dir);
                var dll = Path.Combine(dir, String.Format("{0}.dll", name));

                try
                {
                    if (!File.Exists(dll))
                        throw new CoreException(String.Format("The dll \"{0}\" was not found.", dll));

                    var an = AssemblyName.GetAssemblyName(dll);
                    var assembly = Assembly.Load(an);

                    if (assembly == null)
                        continue;
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                            continue;

                        if (type.GetInterface(pluginType.FullName) == null)
                            continue;

                        var pluginInstance = (IPlugin)Activator.CreateInstance(type);

                        var plugin = new Plugin()
                        {
                            Instance = pluginInstance,
                            DllPath = dir
                        };
                        _pluginCollection.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    AppConsole.Log(String.Format("Error loading plugin {0}: {1} - {2}", name, ex.Message, ex), ConsoleColor.Red);
                }
            }
        }

        internal void Init(IApi _api)
        {
            foreach (var t in _pluginCollection.ToArray())
            {
                try
                {
                    t.Instance.Init(_api, t.DllPath);
                }
                catch (Exception ex)
                {
                    AppConsole.Log(String.Format("Plugin {0} loading failed: {1}", t.Instance.Name, ex.Message), ConsoleColor.Yellow);
                    _pluginCollection.Remove(t);
                }
            }
        }

        internal void Kill()
        {
            AppConsole.Log("Unloading Plugins");
            foreach (var t in _pluginCollection)
            {
                try
                {
                    AppConsole.Log(String.Format("Unloading {0}", t.Instance.Name));
                    t.Instance.Kill();
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}

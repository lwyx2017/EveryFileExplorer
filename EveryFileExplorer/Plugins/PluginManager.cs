using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EveryFileExplorer.Plugins
{
    public class PluginManager
    {
        public Plugin[] Plugins { get; private set; }
        public PluginManager()
        {
            string pluginsDir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Plugins");
            if (!Directory.Exists(pluginsDir))
            {
                Plugins = Array.Empty<Plugin>(); 
                return;
            }
            List<Plugin> plugins = new List<Plugin>();
            foreach (var dllPath in Directory.GetFiles(pluginsDir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    AssemblyName.GetAssemblyName(dllPath);
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    if (Plugin.IsPlugin(assembly))plugins.Add(new Plugin(assembly));
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Please unlock {Path.GetFileName(dllPath)}:\n{ex.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load plugin: {Path.GetFileName(dllPath)}\n{ex.Message}");
                    continue;
                }
            }
            Plugins = plugins.ToArray();
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.Initializer?.OnLoad();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Plugin initialization failed: {plugin.Name}\n{ex.Message}");
                }
            }
        }
    }
}
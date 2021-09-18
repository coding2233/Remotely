using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Remotely.Plugins
{
    class PluginInfo
    {
        public string Name { get; }
        public string FullName { get; }
        public string Desc { get; }
        public string Version { get; }

        public Assembly Assembly { get; }

        public PluginInfo(string name,string fullName,string desc,string version, Assembly assembly)
        {
            this.Name = name;
            this.FullName = fullName;
            this.Desc = desc;
            this.Version = version;
            this.Assembly = assembly;
        }
    }

    public class Plugins
    {
        private string _pluginPath;
        private Dictionary<string, PluginInfo> _pluginAssemblies;
        private int _pluginAdpterVersion;

        public Plugins(string version,string pluginPath= "./plugins")
        {
            _pluginAdpterVersion = VersionToInt(version);
            _pluginPath = pluginPath;
            if (!Directory.Exists(_pluginPath))
            {
                Directory.CreateDirectory(_pluginPath);
            }
            _pluginAssemblies = new Dictionary<string, PluginInfo>();
        }


        private void CheckPlugins()
        {
            var dirs = Directory.GetDirectories(_pluginPath);
            if (dirs != null)
            {
                foreach (var item in dirs)
                {
                    string pluginTxtPath = $"{item}/plugin.txt";
                    if (File.Exists(pluginTxtPath))
                    {
                        var lines = File.ReadAllLines(pluginTxtPath);
                        if (lines != null && lines.Length >= 4)
                        {
                            string version = lines[0].ToLower().Replace(" ","").Replace("version:","");
                            string name = lines[1].ToLower().Replace(" ", "").Replace("name:", "");
                            string fullName = lines[2].ToLower().Replace(" ", "").Replace("fullname:", "");
                            string desc = lines[3].ToLower().Replace(" ", "").Replace("desc:", "");

                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(fullName) && !_pluginAssemblies.ContainsKey(name)
                                && VersionToInt(version) != -404 && VersionToInt(version) >= _pluginAdpterVersion)
                            {
                                string dllPath = $"{item}/{name}.dll";
                                if (File.Exists(dllPath))
                                {
                                    var assembly = Assembly.LoadFile(dllPath);
                                    PluginInfo pluginInfo = new PluginInfo(name,fullName,desc,version,assembly);
                                    _pluginAssemblies.Add(name, pluginInfo);
                                }
                            }
                        }
                    }
                }
            }
        }


        public void CallPlugin(object sender, string method, params object[] args)
        {
            CheckPlugins();

            List<object> senderArgs = new List<object>();
            senderArgs.Add(sender);
            if(args!=null)
                senderArgs.AddRange(args);

            foreach (var item in _pluginAssemblies)
            {
                var assembly = item.Value.Assembly;
                var type = assembly.GetType("NativeCallPlugin");
                if (type != null&& type.GetMethod(method) !=null)
                {
                    type.InvokeMember(method,BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, senderArgs.ToArray());
                }
            }
        }
    
    
        private int VersionToInt(string version)
        {
            version = version.Replace(" ", "").Replace(".", "").Trim();
            if (int.TryParse(version, out int result))
            {
                return result;
            }
            return -404;
        }
    
    }
}

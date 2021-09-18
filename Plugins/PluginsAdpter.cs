using System;

namespace Remotely.Plugins
{
    public static class PluginsAdpter
    {
        public const string Version = "0.0.1";

        private static Plugins _plugins;

        static PluginsAdpter()
        {
            _plugins = new Plugins(Version);
        }

        public static void CallPlugins(object sender, string method,params object[] args)
        {
            _plugins.CallPlugin(sender,method,args);
        }

    }
}

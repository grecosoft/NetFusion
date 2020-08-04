using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Settings.Plugin;
using NetFusion.Test.Plugins;

namespace CoreTests.Settings.Setup
{
    public static class TestSetup
    {
        public static void AddInMemorySettings(IConfigurationBuilder builder)
        {
            var dict = new Dictionary<string, string>
            {
                {"App:MainWindow:Height", "20"},
                {"App:MainWindow:Width", "50"},
                {"App:MainWindows:ValidatedValue", "3" },
                {"App:MainWindow:Dialog:Colors:Frame", "RED"},
                {"App:MainWindow:Dialog:Colors:Title", "DARK_RED"}
            };

            builder.AddInMemoryCollection(dict);
        }

        public static void AddSettingsPlugin(CompositeContainer container, params Type[] settingTypes)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType(settingTypes);
                       
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<SettingsPlugin>();
        }
    }
}
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Implementation of ITypeResolver that delegates to the default
    /// implementation but provides custom logic for Mock Plug-ins.
    /// </summary>
    public class TestTypeResolver : ITypeResolver
    {
        private readonly ITypeResolver _baseResolver = new TypeResolver(new BootstrapLogger());

        public void SetPluginMeta(IPlugin plugin)
        {
            if (plugin.GetType().IsDerivedFrom<MockPlugin>())
            {
                plugin.SetPluginMeta(plugin.AssemblyName, plugin.AssemblyVersion, plugin.Types);
                return;
            }
            
            plugin.SetPluginMeta("test-assembly-name", "test-assembly-version", plugin.Types);
        }

        public void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes)
        {
            _baseResolver.ComposePlugin(plugin, fromPluginTypes);
        }
    }
}

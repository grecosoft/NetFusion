//using System;
//using System.Linq;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using NetFusion.Bootstrap.Container;
//using NetFusion.Bootstrap.Plugins;
//using NetFusion.Test.Plugins;
//
//namespace NetFusion.Test.Modules
//{
//    public static class ModuleTestFixture
//    {
//        /// <summary>
//        /// Returns an instance of a plug-in module associated with a HostPlugin containing
//        /// a set of types.  This can be used to test module functionality where the type of
//        /// plug-in is not important.
//        /// </summary>
//        /// <param name="configBldr">The configuration builder to be used.</param>
//        /// <param name="types">The types to be added to the host plug-in.</param>
//        /// <typeparam name="TModule">The type of the module to be created and associated with plug-in.</typeparam>
//        /// <returns>Instance of module with a context set to the host plugin.</returns>
//        public static TModule SetupModule<TModule>(IConfigurationBuilder configBldr, params Type[] types)
//            where TModule : IPluginModule, new()
//        {
//            var loggerBldr = new LoggerFactory();
//            
//            var compositeApp = new CompositeApplication(configBldr.Build(), loggerBldr);
//            
//            var plugin = new Plugin(new MockHostPlugin());
//            compositeApp.Plugins = new[] { plugin };
//
//            var pluginTypes = types.Select(t => 
//                    new PluginType(
//                        plugin, 
//                        t, 
//                        "Host Unit Tests"))
//                .ToArray();
//            
//            plugin.SetPluginResolvedTypes(
//                pluginTypes,
//                new IPluginModule[] {});
//
//            var moduleUnderTest = new TModule();
//            var context = new ModuleContext(compositeApp, plugin, moduleUnderTest);
//
//            moduleUnderTest.Context = context;
//            moduleUnderTest.SetPluginModuleKnownTypes();
//            
//            return moduleUnderTest;
//        }
//
//        public static TModule SetupModule<TModule>(params Type[] types)
//            where TModule : IPluginModule, new()
//        {
//            return SetupModule<TModule>(new ConfigurationBuilder(), types);
//        }
//
//        public static void SetPluginModuleKnownTypes(this IPluginModule module)
//        {
//            var resolver = new TypeResolver();
//            resolver.SetPluginModuleKnownTypes(module, module.Context.Plugin.PluginTypes);
//        }
//    }
//}
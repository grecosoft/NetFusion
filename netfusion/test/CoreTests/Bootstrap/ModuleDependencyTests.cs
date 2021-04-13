using FluentAssertions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    public class ModuleDependencyTests
    {
        /// <summary>
        /// A module contained within a Core plugin can only have a dependency
        /// on other Core plugin modules implementing an Interface deriving
        /// from IPluginModuleService.
        /// </summary>
        [Fact]
        public void CorePluginModule_CanHaveDependency_OnCoreModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture
                    .Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<HostDependentModule>();

                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddModule<AppDependentModule>();
                        
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<CoreDependentModule>();
                        corePlugin.AddModule<CoreModuleWithDependency>();
                        
                        c.RegisterPlugins(corePlugin, appPlugin, hostPlugin);
                    })
                    .Assert.PluginModule<CoreModuleWithDependency>(m =>
                    {
                        m.CoreDependency.Should().NotBeNull();
                    });
            });
        }

        /// <summary>
        /// A module contained within an Application plugin can only have a dependencies
        /// on other Application and Core plugin modules implementing an Interface deriving
        /// from IPluginModuleService.
        /// </summary>
        [Fact]
        public void ApplicationPluginModule_CanHaveDependency_OnApplicationAndCoreModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture
                    .Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<HostDependentModule>();

                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddModule<AppDependentModule>();
                        appPlugin.AddModule<AppModuleWithDependency>();
                    
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<CoreDependentModule>();

                        c.RegisterPlugins(corePlugin, appPlugin, hostPlugin);
                    })
                    .Assert.PluginModule<AppModuleWithDependency>(m =>
                    {
                        m.CoreDependency.Should().NotBeNull();
                        m.AppDependency.Should().NotBeNull();
                    });
            });
        }

        /// <summary>
        /// A Module contained within a Host plugin can have a dependency 
        /// modules contained within all plugins.
        /// </summary>
        [Fact]
        public void HostPluginModule_CanHaveDependency_OnAllModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture
                    .Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<HostDependentModule>();
                        hostPlugin.AddModule<HostModuleWithDependency>();
                        
                        var appPlugin = new MockAppPlugin();
                        appPlugin.AddModule<AppDependentModule>();

                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<CoreDependentModule>();

                        c.RegisterPlugins(corePlugin, appPlugin, hostPlugin);
                    })
                    .Assert.PluginModule<HostModuleWithDependency>(m =>
                    {
                        m.CoreDependency.Should().NotBeNull();
                        m.AppDependency.Should().NotBeNull();
                        m.HostDependency.Should().NotBeNull();
                    });
            });
        }
        
        private interface IHostDependentService : IPluginModuleService { }
        private interface IAppDependentService : IPluginModuleService { }
        private interface ICoreDependentService : IPluginModuleService { }

        private class HostDependentModule : PluginModule, IHostDependentService { }
        private class AppDependentModule : PluginModule, IAppDependentService { }
        private class CoreDependentModule : PluginModule, ICoreDependentService { }
        
        private class CoreModuleWithDependency : PluginModule
        {
            public ICoreDependentService CoreDependency { get; private set; }
        }

        private class AppModuleWithDependency : PluginModule
        {
            public IAppDependentService AppDependency { get; private set; }
            public ICoreDependentService CoreDependency { get; private set; }
        }

        private class HostModuleWithDependency : PluginModule
        {
            public IHostDependentService HostDependency { get; private set; }
            public IAppDependentService AppDependency { get; private set; }
            public ICoreDependentService CoreDependency { get; private set; }
        }
    }
}
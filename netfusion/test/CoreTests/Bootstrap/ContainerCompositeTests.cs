using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The following tests the constraints that must exist when composing a
    /// CompositeContainer and its structure from a set of plugins. 
    /// </summary>
    public class ContainerStructureTests
    {
        /// <summary>
        /// Not often used by plugin-implementations, but can be used for cases where
        /// instances such as Queues, should be created on a Topic, and be scoped to
        /// a specific application.  In this case, the PluginId of the host plugin is
        /// most often used.
        /// </summary>
        [Fact(DisplayName = "All Plugin Definitions must have and Identity Value")]
        public void AllPluginDefinitions_MustHaveIdentityValue()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange
                    .Container(r =>
                    {
                        var testPlugin = new MockHostPlugin();
                        testPlugin.SetPluginId(null);
                        
                        r.RegisterPlugins(testPlugin);
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-missing-plugin-id");
                    });
            });
        }

        [Fact(DisplayName = "Plugin Identity Values must be Unique")]
        public void PluginIdsValues_MustBeUnique()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        var corePlugin = new MockCorePlugin();
                        
                        hostPlugin.SetPluginId("1");
                        corePlugin.SetPluginId("1");
                        
                        c.RegisterPlugins(hostPlugin, corePlugin);
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-duplicate-plugin-id");
                    });
            });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the composite container structure.
        /// </summary>
        [Fact(DisplayName = "Plugin Name must be Specified")]
        public void PluginName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.SetPluginName(null);
                        
                        c.RegisterPlugins(hostPlugin);
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-missing-metadata");
                    });
            });
        }

        /// <summary>
        /// While every .NET type has an assembly, the only code in the design that
        /// has a dependency on the actual .NET assembly is the class implementing
        /// the ITypeResolver interface.  The type resolver specifies the name of the
        /// type's assembly which is a required property..
        /// </summary>
        [Fact(DisplayName = "Plugin Assembly Name must be Specified")]
        public void PluginAssemblyName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var testPlugin = new MockHostPlugin {AssemblyName = null};
                        c.RegisterPlugins(testPlugin);
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-missing-metadata");
                    });
            });
        }

        /// <summary>
        /// The composite application must have one and only one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application cannot have multiples Host Plugins")]
        public void CompositeApplication_CannotHaveMultiple_AppHostPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin1 = new MockHostPlugin();
                        var hostPlugin2 = new MockHostPlugin();
                        
                        c.RegisterPlugins(hostPlugin1, hostPlugin2);
                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-multiple-host-plugins");
                    });
            });
        }

        /// <summary>
        /// The composite application must have one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application must have one Host Plugin")]
        public void CompositeApplication_MustHaveOne_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(r =>
                    {

                    })
                    .Act.RecordException().ComposeContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.ExceptionId.Should().Be("bootstrap-missing-host-plugin");
                    });
            });
        }

        /// <summary>
        /// The composite application will be constructed from one plugin that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application has Application Host Plugin")]
        public void Composite_Application_Has_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.HostPlugin.Should().NotBeNull();
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several application specific
        /// plugin components.  These plugins contain the main components of the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Application Component Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_AppComponentPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockAppPlugin());
                        c.RegisterPlugins(new MockAppPlugin());
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.AppPlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several core plugins.
        /// These plugins provide reusable services for technical implementations
        /// optionally used by other plugins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plugins")]
        public void CompositeApplication_CanHaveMultiple_CorePlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockCorePlugin());
                        c.RegisterPlugins(new MockCorePlugin());
                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        ca.CorePlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate and organize the implementation.
        /// </summary>
        [Fact(DisplayName = "Plug-In can have Multiple Modules")]
        public void PluginCanHave_MultipleModules()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        
                        var testPlugin = new MockCorePlugin();
                        testPlugin.AddModule<MockPluginTwoModule>();
                        testPlugin.AddModule<MockPluginThreeModule>();

                        c.RegisterPlugins(testPlugin);

                    })
                    .Assert.CompositeAppBuilder(ca =>
                    {
                        var pluginModules = ca.CorePlugins.First().Modules.ToArray();
                        pluginModules.Should().HaveCount(2);
                        pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                        pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                    });
            });
        }

        [Fact]
        public void PluginModule_CanOnlyBe_AddedOnce()
        {
            var exception = Assert.Throws<ContainerException>(() =>
            {
                var plugin = new MockAppPlugin();
                plugin.AddModule<MockPluginOneModule>();
                plugin.AddModule<MockPluginOneModule>();
            });

            exception.ExceptionId.Should().Be("bootstrap-duplicate-module");
        }
    }
}

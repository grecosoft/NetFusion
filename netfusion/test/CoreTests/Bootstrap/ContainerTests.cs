using System;
using System.Linq;
using CoreTests.Bootstrap.Mocks;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    /// <summary>
    /// The following tests the interaction between the AppContainer and the convention
    /// based types used to configure the container.
    /// </summary>
    public class ContainerTests
    {
        [Fact(DisplayName = "All Plug-In Manifests must have Identity value")]
        public void AllPluginManifests_MustHaveIdentityValue()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(r =>
                    {
                        var testPlugin = new MockHostPlugin();
                        testPlugin.SetPluginId(null);
                        
                        r.RegisterPlugins(testPlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {

                    });
            });
        }

        /// <summary>
        /// Plug-in IDs must be unique.  Plug-ins can use these key values to identity information
        /// related to a plug-in. 
        /// </summary>
        [Fact(DisplayName = "Plug-In Id Values must be Unique")]
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
                        
                        c.RegisterPlugins(hostPlugin);
                        c.RegisterPlugins(corePlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("Plug-in identity values must be unique.");
                    });
            });
        }

        /// <summary>
        /// Each plug-in has a name that is used when logging the
        /// container structure.
        /// </summary>
        [Fact(DisplayName = "Plug-In Name must be Specified")]
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
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("All plugins must have AssemblyName and Name values.");
                    });
            });
        }

        /// <summary>
        /// While every .NET type has an assembly, the only code in the design that
        /// has a dependency on the actual .NET assembly is the classes that implement
        /// the ITypeResolver interface.  The type resolver specifies the name of the
        /// type's assembly which must be unique among plug-ins.
        /// </summary>
        [Fact(DisplayName = "Plug-In Assembly Name must be Specified")]
        public void PluginAssemblyName_MustBeSpecified()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var testPlugin = new MockHostPlugin {AssemblyName = null};
                        c.RegisterPlugins(testPlugin);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("All plugins must have AssemblyName and Name values.");
                    });
            });
        }

        /// <summary>
        /// The composite application must have one and only one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application cannot have multiples Application Host Plug-Ins")]
        public void CompositeApplication_CannotHaveMultiple_AppHostPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin1 = new MockHostPlugin();
                        var hostPlugin2 = new MockHostPlugin();
                        
                        c.RegisterPlugins(hostPlugin1);
                        c.RegisterPlugins(hostPlugin2);
                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("There can only be one host plugin.");
                    });
            });
        }

        /// <summary>
        /// The composite application must have one application host plug-in.
        /// </summary>
        [Fact(DisplayName = "Composite Application must have one Application host Plug-In")]
        public void CompositeApplication_MustHaveOne_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(r =>
                    {

                    })
                    .Act.BuildAndStartContainer()
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("he composite application must have one host plugin type.");
                    });
            });
        }

        /// <summary>
        /// The composite application will be constructed from one plug-in that hosts
        /// the application.
        /// </summary>
        [Fact(DisplayName = "Composite Application has Application Host Plug-In")]
        public void Composite_Application_Has_AppHostPlugin()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.HostPlugin.Should().NotBeNull();
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several application
        /// specific plug-in components.  These plug-ins contain the main modules of the 
        /// application.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Application Component Plug-Ins")]
        public void CompositeApplication_CanHaveMultiple_AppComponentPlugins()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                        c.RegisterPlugins(new MockApplicationPlugin());
                        c.RegisterPlugins(new MockApplicationPlugin());
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.AppPlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// The composite application can be constructed from several core plug-ins.
        /// These plug-ins provide reusable services for technical implementations
        /// that can optionally used by other plug-ins.
        /// </summary>
        [Fact(DisplayName = "Composite Application can have Multiple Core Plug-Ins")]
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
                    .Assert.CompositeApp(ca =>
                    {
                        ca.CorePlugins.Should().HaveCount(2);
                    });
            });
        }

        /// <summary>
        /// A plug-in can have multiple modules to separate the configuration for different
        /// provided services.
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
                    .Assert.CompositeApp(ca =>
                    {
                        var pluginModules = ca.CorePlugins.First().Modules.ToArray();
                        pluginModules.Should().HaveCount(2);
                        pluginModules.OfType<MockPluginTwoModule>().Should().HaveCount(1);
                        pluginModules.OfType<MockPluginThreeModule>().Should().HaveCount(1);
                    });
            });
        }


        [Fact(DisplayName = "Container can only be Started Once")]
        public void Container_CanOnlyBeStartedOnce()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnNonInitContainer(c =>
                    {
                        var builtContainer = c.Compose(new TestTypeResolver());
                        builtContainer.Start();
                        builtContainer.Start();
                    })
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("The application container has already been started.");
                    });
            });
        }

        /// <summary>
        /// After the application container is built and started, each plug-in
        /// modules is given an opportunity to start.  This is where each 
        /// module is allowed to initialize any runtime services.  For example,
        /// this is where a service bus plug-in would create the needed queues
        /// and subscribe to consumers.
        /// </summary>
        [Fact(DisplayName = "When Container Started each Plug-in Module is Started")]
        public void WhenAppContainerStarted_EachPluginModuleStarted()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<MockPluginOneModule>();
                        
                        var corePlugin = new MockCorePlugin();
                        corePlugin.AddModule<MockPluginTwoModule>();
                        
                        c.RegisterPlugins(corePlugin);
                        c.RegisterPlugins(hostPlugin);
                        
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        ca.AllModules.OfType<MockPluginModule>()
                            .All(m => m.IsStarted).Should().BeTrue();
                    });
            });
        }

        [Fact(DisplayName = "Non-Started Container cannot be Stopped")]
        public void NonStartedContainer_CannotBeStopped()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(r =>
                    {
                        r.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnNonInitContainer(c =>
                    {
                        c.Compose(new TypeResolver());
                        c.Stop();
                    })
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Contain("The application container has not been started.");
                    });
            });
        }

        /// <summary>
        /// When the application container is disposed, each plug-in module
        /// will be disposed.
        /// </summary>
        [Fact(DisplayName = "When Container disposed plug-in Modules are disposed")]
        public void AppContainerDisposed_PluginModules_AreDisposed()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddModule<MockPluginOneModule>();
                        
                        c.RegisterPlugins(hostPlugin);
                    })
                    .Act.OnContainer(c =>
                    {
                        c.Dispose();
                    })
                    .Assert.CompositeApp(ca =>
                    {
                        var pluginModule = (MockPluginModule)ca.HostPlugin.Modules.First();
                        pluginModule.IsDisposed.Should().BeTrue();
                    });
            });
        }

        /// <summary>
        /// Once the application container is disposed, the associated service
        /// provider can no longer be accessed.
        /// </summary>
        [Fact(DisplayName = "Disposed Container cannot have Service Provider accessed")]
        public void DisposedAppContainer_CannotHave_ServicesAccessed()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture.Arrange.Container(c =>
                    {
                        c.RegisterPlugin<MockHostPlugin>();
                    })
                    .Act.OnContainer(c =>
                    {
                        c.Dispose();
                        c.CreateServiceScope();
                    })
                    .Assert.Exception<ContainerException>(ex =>
                    {
                        ex.Message.Should().Be(
                            "The application container has been disposed and can no longer be accessed.");
                    });
            });
        }
    }
}

using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;

namespace NetFusion.Core.UnitTests.Bootstrap;

/// <summary>
/// The following tests the constraints that must exist when composing a
/// CompositeContainer and its structure from a set of plugins. 
/// </summary>
public class CompositeContainerValidationTests
{
    /// <summary>
    /// Not often used by plugin-implementations, but can be used for cases where
    /// instances such as Queues, should be created on a Topic, and be scoped to
    /// a specific application.  In this case, the PluginId of the host plugin is
    /// most often used.
    /// </summary>
    [Fact]
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
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.ExceptionId.Should().Be("bootstrap-missing-plugin-id");
                });
        });
    }

    [Fact]
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
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.ExceptionId.Should().Be("bootstrap-duplicate-plugin-id");
                });
        });
    }

    /// <summary>
    /// Each plug-in has a name that is used when logging the composite container structure.
    /// </summary>
    [Fact]
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
                .Assert.Exception<BootstrapException>(ex =>
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
    [Fact]
    public void PluginAssemblyName_MustBeSpecified()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(c =>
                {
                    var testPlugin = new MockHostPlugin();
                    testPlugin.SetPluginMeta(null, "1.0.0", Array.Empty<Type>());
                    c.RegisterPlugins(testPlugin);
                })
                .Act.RecordException().ComposeContainer()
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.ExceptionId.Should().Be("bootstrap-missing-metadata");
                });
        });
    }

    /// <summary>
    /// The composite application must have one and only one application host plug-in.
    /// </summary>
    [Fact]
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
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.ExceptionId.Should().Be("bootstrap-multiple-host-plugins");
                });
        });
    }

    /// <summary>
    /// The composite application must have one application host plug-in.
    /// </summary>
    [Fact]
    public void CompositeApplication_MustHaveOne_AppHostPlugin()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(_ =>
                {

                })
                .Act.RecordException().ComposeContainer()
                .Assert.Exception<BootstrapException>(ex =>
                {
                    ex.ExceptionId.Should().Be("bootstrap-missing-host-plugin");
                });
        });
    }
}
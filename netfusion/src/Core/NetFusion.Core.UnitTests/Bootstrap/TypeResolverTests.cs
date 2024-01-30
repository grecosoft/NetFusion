using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.UnitTests.Bootstrap;

public class TypeResolverTests
{
    /// <summary>
    /// The CompositeContainer does not have a dependency on .net's Assembly.  This dependency
    /// has been moved to the TypeResolver.  This allows for simplified unit-testing.
    /// </summary>
    [Fact]
    public void SetPluginMetadata_FromAssembly()
    {
        // Arrange:
        var typeResolver = new TypeResolver();
        var plugin = new MockResolvedPlugin();
            
        // Test:
        typeResolver.SetPluginMeta(plugin);
            
        // Assert:
        plugin.AssemblyName.Should().Be(GetType().Assembly.FullName);
        plugin.AssemblyVersion.Should().Be(GetType().Assembly.GetName().Version?.ToString());
        plugin.Types.Should().NotBeNull();
        plugin.Types.Should().Contain(typeof(MockPluginType));
    }

    /// <summary>
    /// Core plugins define abstract types deriving from IPluginKnownType.  Other plugins
    /// provide concrete implementations.  The Core plugin can find all such implementations
    /// by defining a IEnumerable module property of the derived IPluginKnownType type.
    /// </summary>
    [Fact]
    public void PopulatesModuleProperties_ReferencingKnownTypes()
    {
        // Arrange:
        var typeResolver = new TypeResolver();
        var plugin = new MockResolvedPlugin();
            
        // Act:
        typeResolver.ComposePlugin(plugin, new[]
        {
            typeof(MockKnownConcreteTypeOne),
            typeof(MockKnownConcreteTypeTwo)
        });

        // Assert:
        var mockModule = plugin.Modules.OfType<MockPluginModule>().First();
        mockModule.FoundInstances.Should().NotBeNull();
        mockModule.FoundInstances.Should().HaveCount(2);
        mockModule.FoundInstances.Any(i => i.Value == "ONE").Should().BeTrue();
        mockModule.FoundInstances.Any(i => i.Value == "TWO").Should().BeTrue();
    }

    /// <summary>
    /// Details for all set module properties are recorded for logging purposes.
    /// </summary>
    [Fact]
    public void PopulatesModuleProperties_RecordedForLogging()
    {
        // Arrange:
        var typeResolver = new TypeResolver();
        var plugin = new MockResolvedPlugin();
            
        // Act:
        typeResolver.ComposePlugin(plugin, new[]
        {
            typeof(MockKnownConcreteTypeOne),
            typeof(MockKnownConcreteTypeTwo)
        });
            
        // Assert:
        var module = (IPluginModule)plugin.Modules.OfType<MockPluginModule>().First();

        module.KnownTypeProperties.Should().NotBeNull();
        module.KnownTypeProperties.Should().HaveCount(1);

        var knownTypeProp = typeof(MockPluginModule).GetProperty("FoundInstances");
        if (knownTypeProp == null)
        {
            throw new NullReferenceException("KnownType property not found.");
        }
            
        var (knownType, concreteTypes) = module.KnownTypeProperties[knownTypeProp];
        Assert.True(knownType == typeof(IMockKnowType), "KnownType not correctly identified");
        concreteTypes.Should().HaveCount(2);
        concreteTypes.Should().Contain(ct => ct == typeof(MockKnownConcreteTypeOne));
        concreteTypes.Should().Contain(ct => ct == typeof(MockKnownConcreteTypeTwo));
    }

    private class MockResolvedPlugin : PluginBase
    {
        public override string PluginId => "unique-plugin-id";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Mock Plugin";

        public MockResolvedPlugin()
        {
            AddModule<MockPluginModule>();
        }
    }

    public class MockPluginType;

    public class MockPluginModule : PluginModule
    {
        public IEnumerable<IMockKnowType> FoundInstances { get; set; }
    }
        
    public interface IMockKnowType : IPluginKnownType
    {
        string Value { get; }
    }

    public class MockKnownConcreteTypeOne : IMockKnowType
    {
        public string Value => "ONE";
    }

    public class MockKnownConcreteTypeTwo : IMockKnowType
    {
        public string Value => "TWO";
    }
}
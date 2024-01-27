using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.UnitTests.Bootstrap.Mocks;

public class MockComposedModule : PluginModule
{
    /// <summary>
    /// Any property on a derived PluginModule, that is an enumeration
    /// of a derived IKnownPluginType, will automatically be populated
    /// with instances of deriving concrete classes.  In this scenario,
    /// this property would be populated with the following:
    /// 
    ///     MockTypeOneBasedOnKnownType
    ///     MockTypeTwoBasedOnKnownType
    ///     MockTypeThreeBasedOnKnownType
    ///
    /// This allows one plugin, defining a derived IKnownPluginType, to
    /// locate class implementations defined by other plugins.
    /// </summary>
    public IEnumerable<IMockKnownType> ImportedTypes { get; set; }
}
    
public interface IMockKnownType : IPluginKnownType;

public class MockTypeOneBasedOnKnownType : IMockKnownType;

public class MockTypeTwoBasedOnKnownType : IMockKnownType;

public class MockTypeThreeBasedOnKnownType : IMockKnownType;
    
public class MockTypeFourBasedOnKnownType : IMockKnownType;
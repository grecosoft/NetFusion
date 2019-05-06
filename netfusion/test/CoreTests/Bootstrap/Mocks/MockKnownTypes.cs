using System.Collections.Generic;
using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Plugins;

namespace CoreTests.Bootstrap.Mocks
{
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
        public IEnumerable<MockKnownType> ImportedTypes { get; set; }
    }
    
    public abstract class MockKnownType : IKnownPluginType
    {
    }

    public class MockTypeOneBasedOnKnownType : MockKnownType
    {
    }

    public class MockTypeTwoBasedOnKnownType : MockKnownType
    {
    }

    public class MockTypeThreeBasedOnKnownType : MockKnownType
    {
    }
}

using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Core.Tests.Bootstrap.Mocks
{
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
    
using NetFusion.Base.Plugins;

namespace CoreTests.Bootstrap.Mocks
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

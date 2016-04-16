using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Core.Tests.Bootstrap.Mocks
{
    public class MockActivatedType : IComponentActivated
    {
        public bool IsActivated { get; private set; }

        public void OnActivated()
        {
            this.IsActivated = true;
        }
    }
}

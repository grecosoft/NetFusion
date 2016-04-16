using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Tests.Core.Bootstrap.Mocks
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

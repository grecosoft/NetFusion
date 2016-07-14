using Autofac;
using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;

namespace NetFusion.Tests.Core.Bootstrap.Mocks
{
    public abstract class MockPluginModule : PluginModule
    {
        public bool IsDisposed { get; private set; }
        public bool IsStarted { get; private set; }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);
            this.IsDisposed = true;
        }

        public override void StartModule(ILifetimeScope scope)
        {
            this.IsStarted = true;
        }
    }

    public interface IMockPluginOneModule : IPluginModuleService
    {

    }

    public class MockPluginOneModule : MockPluginModule,
        IMockPluginOneModule
    {

    }

    public class MockPluginTwoModule : MockPluginModule
    {

    }

    public class MockPluginThreeModule : MockPluginModule
    {

    }

    public class MockLoggingModule : MockPluginModule
    {
        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Log-Msg"] = "Module-Added-Log";
        }
    }

    public class MockComposedModule : MockPluginModule
    {
        public IEnumerable<MockKnownType> ImportedTypes { get; set; }
    }

}

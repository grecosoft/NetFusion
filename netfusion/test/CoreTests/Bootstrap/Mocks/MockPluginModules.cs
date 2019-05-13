using System.Threading.Tasks;
using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;
using System;

namespace CoreTests.Bootstrap.Mocks
{
    public abstract class MockPluginModule : PluginModule
    {
        public bool IsDisposed { get; private set; }
        public bool IsStarted { get; private set; }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);
            IsDisposed = true;
        }

        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            IsStarted = true;

            return base.OnStartModuleAsync(services);
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
}

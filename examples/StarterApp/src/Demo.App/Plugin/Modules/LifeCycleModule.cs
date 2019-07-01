using System;
using System.Threading.Tasks;
using Demo.Core;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin.Modules
{
    public class LifeCycleModule: PluginModule
    {
        public ICalculatorModule CalcModule { get; private set; }

        public override void Initialize()
        {
            Context.Logger.LogDebug("--Initialize Called--");   
            Context.Logger.LogDebug(Context.AppHost.Name);
            Context.Logger.LogDebug(Context.Plugin.Name); 

            if (CalcModule != null) 
            {
                Context.Logger.LogDebug("Module Reference Set!");
            }
        }

        public override void Configure()
        {
            Context.Logger.LogDebug("--Configure Called--");
        }
        
        protected override Task OnStartModuleAsync(IServiceProvider services)
        {
            Context.Logger.LogDebug("--OnStartModuleAsync Called--");
            return base.OnStartModuleAsync(services);
        }

        protected override Task OnRunModuleAsync(IServiceProvider services)
        {
            Context.Logger.LogDebug("--OnRunModuleAsync Called--");
            return base.OnRunModuleAsync(services);
        }

        protected override Task OnStopModuleAsync(IServiceProvider services)
        {
            Context.Logger.LogDebug("--OnStopModuleAsync Called--");
            return base.OnStopModuleAsync(services);
        }
    }
}
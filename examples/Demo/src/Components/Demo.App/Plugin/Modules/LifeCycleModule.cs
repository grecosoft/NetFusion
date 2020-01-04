using System;
using System.Threading.Tasks;
using Demo.Core.Plugin;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin.Modules
{
    public class LifeCycleModule: PluginModule
    {
        public ICalculatorModule CalcModule { get; private set; }

        public override void Initialize()
        {
            Context.BootstrapLogger.Add(LogLevel.Debug, "--Initialize Called--");   
            Context.BootstrapLogger.Add(LogLevel.Debug, Context.AppHost.Name);
            Context.BootstrapLogger.Add(LogLevel.Debug, Context.Plugin.Name); 

            if (CalcModule != null) 
            {
                Context.BootstrapLogger.Add(LogLevel.Debug, "Module Reference Set!");
            }
        }

        public override void Configure()
        {
            Context.BootstrapLogger.Add(LogLevel.Debug, "--Configure Called--");
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
using System;
using System.Threading.Tasks;
using Demo.Core.Plugin;
using Microsoft.Extensions.Logging;
using NetFusion.Base;
using NetFusion.Bootstrap.Plugins;

namespace Demo.App.Plugin.Modules
{
    public class LifeCycleModule: PluginModule
    {
        public ICalculatorModule CalcModule { get; private set; }

        public override void Initialize()
        {
            NfExtensions.Logger.Log<LifeCycleModule>(LogLevel.Debug, "--Initialize Called--");   
            NfExtensions.Logger.Log<LifeCycleModule>(LogLevel.Debug, Context.AppHost.Name);
            NfExtensions.Logger.Log<LifeCycleModule>(LogLevel.Debug, Context.Plugin.Name); 

            if (CalcModule != null) 
            {
                NfExtensions.Logger.Log<LifeCycleModule>(LogLevel.Debug, "Module Reference Set!");
            }
        }

        public override void Configure()
        {
            NfExtensions.Logger.Log<LifeCycleModule>(LogLevel.Debug, "--Configure Called--");
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
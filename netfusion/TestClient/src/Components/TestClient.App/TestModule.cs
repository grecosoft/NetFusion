namespace TestClient.App
{
    using Claims.Notes.App.Services;
    using Microsoft.Extensions.DependencyInjection;
    using NetFusion.Bootstrap.Dependencies;
    using NetFusion.Bootstrap.Plugins;
    using TestClient.Domain.Services;

    public class TestModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
//            services.AddSingleton<ITestService, TestService2>();
        }

        public override void ScanPlugin(ITypeCatalog catalog)
        {
            catalog.AsImplementedInterface("Service2", ServiceLifetime.Singleton);
        }
    }
}
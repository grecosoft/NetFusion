using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Serialization;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.UnitTests.Bus.Mocks;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Integration.UnitTests;

public static class BootstrapExtensions
{
    public static ContainerArrange TestMessageBus(this ContainerArrange arrange,
        IPlugin? testSpecificPlugin = null)
    {
        var hostPlugin = new MockHostPlugin();
        hostPlugin.AddPluginType<TestBusRouter>();
        
        var pluginUnderTest = new MockCorePlugin();
        pluginUnderTest.AddModule<TestEntityPluginModule>();

        arrange.Services(s => s.AddSingleton<ISerializationManager, TestSerializationManager>());
        arrange.Container(c =>
        {
            c.RegisterPlugins(hostPlugin, pluginUnderTest, new MessagingPlugin());
            if (testSpecificPlugin != null)
            {
                c.RegisterPlugins(testSpecificPlugin);
            }
        });

        return arrange;
    }

    public static void AssertStrategiesOfType<TStrategy>(this TestEntityPluginModule module,
        Action<TStrategy> assert)
        where TStrategy: IBusEntityStrategy
    {
        foreach (var strategy in module.TestBusEntities.SelectMany(e => e.Strategies).OfType<TStrategy>())
        {
            assert(strategy);
        }
    }
}
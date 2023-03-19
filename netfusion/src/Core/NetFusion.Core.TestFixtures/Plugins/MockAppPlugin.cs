using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.TestFixtures.Plugins;

/// <summary>
/// Mock application component plug-in that can be used for testing.
/// The composite container can have multiple associated application
/// components.  Components in this type of plug-in are specific 
/// to the domain of the application.  Domain Entities, Aggregates, 
/// and Services such examples.
/// </summary>
public class MockAppPlugin : MockPlugin
{
    public MockAppPlugin() : base(PluginTypes.AppPlugin)
    {
            
    }
}
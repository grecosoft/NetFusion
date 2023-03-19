 using NetFusion.Core.Bootstrap.Plugins;

 namespace NetFusion.Core.TestFixtures.Plugins; 

 /// <summary>
 /// Mock core plug-in that can be used for testing.  In the composite container,
 /// core plug-ins implement cross-cutting concerns and implement specific technical
 /// details used to support the application domain.
 /// </summary>
 public class MockCorePlugin : MockPlugin
 {
     public MockCorePlugin() : base(PluginTypes.CorePlugin)
     {
            
     }
 }
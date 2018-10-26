using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Messaging
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public Manifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/src/Core/NetFusion.Messaging";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/core.messaging.overview";
        }

        public string PluginId => "4576D809-E216-4C03-BE43-737728047BAA";
        public string Name => "Messaging Plug-in"; 

        public string Description =>
             "Contains common implementation for handling Commands, Domain-Events and Queries in-process that " +
             "can be extended by other plug-ins to publish Commands and Domain-Event messages out of process."; 
    }
}

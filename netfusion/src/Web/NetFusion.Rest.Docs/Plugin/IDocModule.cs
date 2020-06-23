using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Plugin.Configs;

namespace NetFusion.Rest.Docs.Plugin
{
    public interface IDocModule : IPluginModuleService
    {
        RestDocConfig RestDocConfig { get; }
        public IEnumerable<IDocDescription> GetDocDescriptions();
    }
}
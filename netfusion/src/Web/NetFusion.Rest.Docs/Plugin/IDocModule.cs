using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;

namespace NetFusion.Rest.Docs.Plugin
{
    public interface IDocModule : IPluginModuleService
    {
        public IEnumerable<IDocDescription> GetDocDescriptions();
    }
}
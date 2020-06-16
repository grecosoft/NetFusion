using System;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core;

namespace NetFusion.Rest.Docs.Plugin
{
    public interface IDocModule : IPluginModuleService
    {
        void ApplyDescriptions<T>(Action<T> description)
            where T : IDocDescription;
    }
}
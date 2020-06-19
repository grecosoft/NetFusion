using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;

namespace NetFusion.Rest.Docs.Plugin
{
    public interface IDocModule : IPluginModuleService
    {
        void ApplyDescriptions<T>(IDictionary<string, object> context, Action<T> description)
            where T : class, IDocDescription;
    }
}
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    public interface IComposite
    {
        IEnumerable<Plugin> Plugins { get; }
        Plugin GetPluginForType(Type type);
    }
}

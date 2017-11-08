using NetFusion.Base.Plugins;
using System.Collections.Generic;

namespace NetFusion.Mapping
{
    public interface IMappingStrategyFactory : IKnownPluginType
    {
        IEnumerable<IMappingStrategy> GetStrategies();
    }
}

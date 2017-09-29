using NetFusion.Base.Plugins;
using System.Collections.Generic;

namespace NetFusion.Utilities.Mapping
{
    public interface IMappingStrategyFactory : IKnownPluginType
    {
        IEnumerable<IMappingStrategy> GetStrategies();
    }
}

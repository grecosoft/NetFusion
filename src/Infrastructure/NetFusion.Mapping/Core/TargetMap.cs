using NetFusion.Mapping;
using System;

namespace NetFusion.Mapping.Core
{
    /// <summary>
    /// Stores for a given source and target type the corresponding mapping strategy.
    /// </summary>
    public class TargetMap
    {
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public Type StrategyType { get; set; }
        public IMappingStrategy StrategyInstance { get; set; }
    }
}

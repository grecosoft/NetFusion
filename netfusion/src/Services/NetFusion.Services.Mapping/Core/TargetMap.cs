using System;

namespace NetFusion.Services.Mapping.Core;

/// <summary>
/// Stores for a given source and target type the corresponding mapping strategy.
/// </summary>
public class TargetMap
{
    /// <summary>
    /// The source type the strategy maps from.
    /// </summary>
    public Type SourceType { get; }
    
    /// <summary>
    /// The target type the strategy maps to.
    /// </summary>
    public Type TargetType { get; }
    
    /// <summary>
    /// The type of the strategy providing the mapping from source to target types.
    /// If specified, the type of the strategy is registered within the dependency
    /// injection container and instantiated per lifetime scope. 
    /// </summary>
    public Type? StrategyType { get; }
    
    /// <summary>
    /// Single instance of the strategy providing the mapping from source to target types.
    /// </summary>
    public IMappingStrategy? StrategyInstance { get; }

    public TargetMap(Type sourceType, Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    public TargetMap(Type sourceType, Type targetType, Type strategyType) :
        this(sourceType, targetType)
    {
        StrategyType = strategyType;
    }
    
    public TargetMap(Type sourceType, Type targetType, IMappingStrategy strategyInstance) :
        this(sourceType, targetType)
    {
        StrategyInstance = strategyInstance;
    }
}
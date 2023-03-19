using System;

namespace NetFusion.Services.Mapping;

/// <summary>
/// Static class used to create an instance of generic DelegateMap class for a
/// specific source and target types.  This allows the instance to be created
/// based on inference of the mapping function parameter types. 
/// </summary>
public static class DelegateMap
{
    /// <summary>
    /// Called to specify a delegate for mapping between a source and target type.
    /// </summary>
    /// <param name="mapping">Function for mapping between the source and target type.</param>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <returns>Instance of a DelegateMap used to convert between a source and target type.</returns>
    public static DelegateMap<TSource, TTarget> Map<TSource, TTarget>(Func<TSource, TTarget> mapping)
        where TSource: class 
        where TTarget: class
    {
        return new DelegateMap<TSource, TTarget>(mapping);
    }
    
    /// <summary>
    /// Called to specify a delegate for mapping between a source and target type.
    /// </summary>
    /// <param name="mapping">Function for mapping between the source and target type.</param>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <returns>Instance of a DelegateMap used to convert between a source and target type.</returns>
    public static DelegateMap<TSource, TTarget> Map<TSource, TTarget>(Func<TSource, IObjectMapper, TTarget> mapping)
        where TSource: class 
        where TTarget: class
    {
        return new DelegateMap<TSource, TTarget>(mapping);
    }
}
    
/// <summary>
/// A derived mapping strategy specified as function delegates for mapping between a source
/// and target type.  An instance of this class is created by invoking a method on the static
/// DelegateMap class.  This provides a compact way of defining mappings when implementing a
/// IMappingStrategyFactory.  The factory implementation can "yield return" multiple DelegateMap
/// instances for mapping between a set of types.  For more complex mappings between a specific
/// source and target types, a strategy can be derived directly from the generic MappingStrategy
/// class.
/// </summary>
/// <typeparam name="TSource">The source type to be mapped.</typeparam>
/// <typeparam name="TTarget">The target type to be mapped into.</typeparam>
public sealed class DelegateMap<TSource, TTarget> : MappingStrategy<TSource, TTarget> 
    where TSource : class
    where TTarget : class
{
    private readonly Func<TSource, TTarget>? _mapping;
    private readonly Func<TSource, IObjectMapper, TTarget>? _mappingEx;

    internal DelegateMap(Func<TSource, TTarget> mapping)
    {
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }
    
    internal DelegateMap(Func<TSource, IObjectMapper, TTarget> mapping)
    {
        _mappingEx = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }
    
    // Base method overrides implemented by delegating to the specified
    // mapping delegates.
    protected override TTarget SourceToTarget(TSource source) => 
        _mapping?.Invoke(source) ?? _mappingEx?.Invoke(source, Mapper) ??
            throw new InvalidOperationException("Invalid Delete Map");
}
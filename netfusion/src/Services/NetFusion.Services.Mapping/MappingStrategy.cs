using System;

namespace NetFusion.Services.Mapping;

/// <summary>
/// Base generic class used to define mapping strategies between source and target types.
/// </summary>
public abstract class MappingStrategy<TSource, TTarget> : IMappingStrategy<TSource, TTarget>
    where TSource : class
    where TTarget : class
{
    public Type SourceType => typeof(TSource);
    public Type TargetType => typeof(TTarget);
    
    // Provides access to the object-mapper for use by derived mapping strategies
    // to map child object references.
    protected IObjectMapper Mapper => _mapper ?? throw new NullReferenceException("Mapper not initialized");

    private IObjectMapper? _mapper;

    public object Map(IObjectMapper mapper, object obj)
    {
        _mapper = mapper;
        return SourceToTarget((TSource)obj);
    }

    /// <summary>
    /// Overridden by a derived mapping strategy to map source to target object.
    /// </summary>
    /// <param name="source">The source object to be mapped.</param>
    /// <returns>Instance of the target object.</returns>
    protected abstract TTarget SourceToTarget(TSource source);
    
}
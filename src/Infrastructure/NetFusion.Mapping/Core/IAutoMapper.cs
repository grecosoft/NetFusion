namespace NetFusion.Mapping.Core
{
    /// <summary>
    /// Interface implemented by the host application used to delegate to the
    /// mapping library of choice.  This interface is used by the plug-in when
    /// needed to automatically map properties from one object instance to another.
    /// </summary>
    public interface IAutoMapper
    {
        /// <summary>
        /// Called when the plug-in needs to map an object to another type.
        /// </summary>
        /// <typeparam name="TTarget">The target type the passed object should be mapped into.</typeparam>
        /// <param name="source">The object to be mapped to the target type.</param>
        /// <returns>Mapped instance of the object.</returns>
        TTarget Map<TTarget>(object source) where TTarget : class;
    }
}

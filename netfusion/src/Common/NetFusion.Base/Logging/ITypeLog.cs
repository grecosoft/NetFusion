namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Implemented by a type to determine what state it should log.
    /// </summary>
    public interface ITypeLog
    {
        /// <summary>
        /// Should return subset of a type's state to be logged.
        /// </summary>
        /// <returns>State to be logged.</returns>
        object Log();
    }
}
namespace NetFusion.EntityFramework
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Interface used to identify entity based database contexts.
    /// </summary>
    public interface IEntityDbContext
    {
        /// <summary>
        /// Reference to the base EntityFramework context.
        /// </summary>
        DbContext Context { get; }
    }
}
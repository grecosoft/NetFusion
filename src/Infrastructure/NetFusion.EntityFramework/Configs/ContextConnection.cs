namespace NetFusion.EntityFramework.Configs
{
    /// <summary>
    /// Provided by the application host to specify the database connection
    /// string that should be used for the name of a given Entity Framework
    /// context.
    /// </summary>
    public class ContextConnection
    {
        /// <summary>
        /// The name of the class deriving from IEntityDbContext
        /// used as identity the connection.
        /// </summary>
        public string ContextName { get; set; }

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}

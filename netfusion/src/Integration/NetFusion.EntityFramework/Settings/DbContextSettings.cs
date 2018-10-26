namespace NetFusion.EntityFramework.Settings
{
    public class DbContextSettings
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
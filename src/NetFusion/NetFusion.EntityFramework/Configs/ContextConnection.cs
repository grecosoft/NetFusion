namespace NetFusion.EntityFramework.Configs
{
    /// <summary>
    /// Provided by the application host to specify the database connection
    /// string that should be used for the name of a given Entity Framework
    /// context.
    /// </summary>
    public class ContextConnection
    {
        public string ContextName { get; set; }
        public string ConnectionString { get; set; }
    }
}

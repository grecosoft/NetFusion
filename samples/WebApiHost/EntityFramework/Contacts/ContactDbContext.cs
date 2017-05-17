using NetFusion.EntityFramework;

namespace WebApiHost.EntityFramework.Contacts
{
    /// <summary>
    /// Example Entity Framework context.
    /// </summary>
    public class ContactDbContext : EntityDbContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Connection string sent when injected into repository.</param>
        /// <param name="mappings">The mappings within the same namespace or child namespace of the context.</param>
        public ContactDbContext(string connectionString, IEntityTypeMapping[] mappings)
            : base(connectionString, mappings)
        {

        }
    }
}

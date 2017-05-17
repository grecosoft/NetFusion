using Microsoft.EntityFrameworkCore;
using NetFusion.EntityFramework;
using WebApiHost.EntityFramework.Models;

namespace WebApiHost.EntityFramework.Contacts.Mappings
{
    /// <summary>
    /// Example mapping associated with the ContactDbContext since it is in 
    /// a child namespace.
    /// </summary>
    public class ContactMappings : EntityTypeMapping
    {
        /// <summary>
        /// Called when the context is created.
        /// </summary>
        /// <param name="modelBuilder">Referee to the class used to register mappings.</param>
        public override void AddMappings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>();
        }
    }
}

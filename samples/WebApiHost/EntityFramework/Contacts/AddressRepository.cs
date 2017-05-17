using Microsoft.EntityFrameworkCore;
using NetFusion.EntityFramework;
using System.Threading.Tasks;
using WebApiHost.EntityFramework.Models;

namespace WebApiHost.EntityFramework.Contacts
{
    /// <summary>
    /// Example Entity Framework based repository.
    /// </summary>
    public class AddressRepository : IAddressRepository
    {
        private readonly IEntityContext<ContactDbContext> _context;
        private DbSet<Address> AddressDbSet { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The injected context used to query database.</param>
        public AddressRepository(IEntityContext<ContactDbContext> context)
        {
            _context = context;

            this.AddressDbSet = _context.Set<Address>();
        }

        /// <summary>
        /// Returns all the addresses.
        /// </summary>
        /// <returns></returns>
        public Task<Address[]> ReadAddresses()
        {
            return AddressDbSet.ToArrayAsync();
        }
    }
}

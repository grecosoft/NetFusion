using System.Threading.Tasks;
using WebApiHost.EntityFramework.Models;

namespace WebApiHost.EntityFramework
{
    /// <summary>
    /// Example Entity Framework repository
    /// </summary>
    public interface IAddressRepository
    {
        /// <summary>
        /// Returns all the addresses.
        /// </summary>
        /// <returns>Address data entities.</returns>
        Task<Address[]> ReadAddresses();
    }
}

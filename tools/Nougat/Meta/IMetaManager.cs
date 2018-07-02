using System.Collections.Generic;
using System.Threading.Tasks;
using Nougat.Projects;

namespace Nougat.Meta
{
    /// <summary>
    /// Responsible for download the latest package information 
    /// from the Nuget server.
    /// </summary>
    public interface IMetaManager
    {
        /// <summary>
        /// Sets the package Nuget data for a list of packages.
        /// </summary>
        /// <param name="installedPackages">List of packages.</param>
        Task SetPackageMeta(IEnumerable<Package> installedPackages);
    }
}
using System.Threading.Tasks;

namespace Nougat.Projects
{
    /// <summary>
    /// Response for loading all packages referenced by 
    /// a list of project files.
    /// </summary>
    public interface IProjectManager
    {
        /// <summary>
        /// The root directory to scan for all project files.
        /// </summary>
        string ProjectRootDir { get; }
   
        /// <summary>
        /// A list of all the found project files. 
        /// </summary>
        string[] ProjectFiles { get; }

        /// <summary>
        /// Returns an unique list of referenced packages specifying
        /// which ones have conflicts and ones for which there is a
        /// newer version on the Nuget server.
        /// </summary>
        /// <returns>List of packages</returns>
        Task<Package[]> GetInstalledPackages();
    }
}
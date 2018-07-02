using Nougat.Projects;

namespace Nougat.Updates
{
    /// <summary>
    /// Responsible for finding lists of packages meeting a specific
    /// criteria and for applying updates.
    /// </summary>
    public interface IUpdateManager
    {
        /// <summary>
        /// Returns a list of packages for which there are multiple versions.  
        /// All packages not having the max installed version are returned.
        /// </summary>
        /// <param name="packages">The list of source packages.</param>
        /// <returns>List of packages having multiple versions.</returns>
        ProjectUpdate GetConflictPackageList(Package[] packages);
        
        /// <summary>
        /// Returns a list of packages for which there is a more recent
        /// version on the Nuget server than installed.
        /// </summary>
        /// <param name="packages">The list of source packages.</param>
        /// <returns>List of packages having never released versions.</returns>
        ProjectUpdate GetUpdatePackageList(Package[] packages);
        
        /// <summary>
        /// Provided a list of packages to updates,  updates all project files
        /// to reference the specified updated version.
        /// </summary>
        /// <param name="projectUpdate">A package and a list of packager files
        /// to be updated.</param>
        /// <param name="namesStartsWith">Limits the update to packages starting
        /// with the specified names.</param>
        void ApplyProjectUpdate(ProjectUpdate projectUpdate, string[] namesStartsWith);
    }
}
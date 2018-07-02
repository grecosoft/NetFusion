using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nougat.Projects;

namespace Nougat.Updates
{
    public class UpdateManager : IUpdateManager
    {
        private ILogger Logger { get; set; }

        private UpdateManager() {}

        public static UpdateManager Create(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            return new UpdateManager {
                Logger = loggerFactory.CreateLogger<UpdateManager>()
            };
        }

        // All packages where there are multiple installed versions and
        // the installed version does not equal the max installed version.
        public ProjectUpdate GetConflictPackageList(Package[] packages)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            var conflicts = packages.Where(p => p.HasConflict).ToArray();  
            return CreatePackageUpdate(conflicts, p => p.MaxVersion);
        }

        // All packages where the installed version is not the latest
        // version reported from Nuget search.
        public ProjectUpdate GetUpdatePackageList(Package[] packages)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            var updates = packages.Where(p => p.HasUpdate).ToArray();
            return CreatePackageUpdate(updates, p => p.LatestVersion);
        }

        private ProjectUpdate CreatePackageUpdate(Package[] packages, Func<Package, string> updateToVersion)
        {
            var projectFiles = packages.SelectMany(p => p.Projects)
                .Distinct().ToArray();

            var updates = packages.Select(
                    p => new PackageUpdate(p.Name, p.Version, updateToVersion(p)))
                .ToArray();

            return new ProjectUpdate(projectFiles, updates);
        }

        public void ApplyProjectUpdate(ProjectUpdate projectUpdate, string[] namesStartsWith)
        {
            if (projectUpdate == null)
                throw new ArgumentNullException(nameof(projectUpdate));

            bool projectUpdated;

            foreach (string projectFile in projectUpdate.ProjectFiles)
            {
                projectUpdated = false;
                Logger.LogTrace("Updating Project: {filePath}", projectFile);

                var project = Project.Load(projectFile);
                
                var packagesToUpdate = namesStartsWith.Length == 0 ? projectUpdate.Packages : 
                    projectUpdate.Packages
                        .Where(p => namesStartsWith.Any(n => p.Name.StartsWith(n)))
                        .ToArray();

                foreach(PackageUpdate pkgUpdate in packagesToUpdate)
                {
                    var currPkg = project.Packages.FirstOrDefault(p => 
                        p.Name == pkgUpdate.Name && 
                        p.Version == pkgUpdate.CurrentVersion);

                    if (currPkg != null)
                    {
                        projectUpdated = true;

                        Logger.LogTrace("Updating Package {package}: from {currentVersion} to {updatedVersion}",
                            currPkg.Name, currPkg.Version, pkgUpdate.UpdatedVersion);

                        currPkg.Version = pkgUpdate.UpdatedVersion;                
                    }
                }

                if (projectUpdated)
                {
                    project.Save();
                }
            }
        }
    }
}
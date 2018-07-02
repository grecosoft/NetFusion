using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nougat.Meta;

namespace Nougat.Projects
{
    public class ProjectManager : IProjectManager
    {
        private ILogger Logger { get; set; }
        private IMetaManager MetaManager { get; set; }

        public string ProjectRootDir { get; private set; }
        public string[] ProjectFiles { get; private set; }

        private ProjectManager() {}

        public static IProjectManager Create(ILoggerFactory loggerFactory,
            IMetaManager metaManager, string projectRootDir) 
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (metaManager == null) throw new ArgumentNullException(nameof(metaManager));

            if (string.IsNullOrWhiteSpace(projectRootDir)) throw new ArgumentException(
                "Project Directory not specified.", nameof(projectRootDir));

            var logger = loggerFactory.CreateLogger<ProjectManager>();

            logger.LogInformation("Searching {directory} for projects.", projectRootDir);
   
            return new ProjectManager {
                Logger = logger,
                ProjectRootDir = projectRootDir,
                ProjectFiles = GetProjectFilePaths(projectRootDir),
                MetaManager = metaManager
            };        
        }

        private static string[] GetProjectFilePaths(string projectDirPath)
        {
            return Directory.GetFiles(projectDirPath, "*.csproj", SearchOption.AllDirectories);
        }   

        public async Task<Package[]> GetInstalledPackages()
        {
            var installedPackages = new Dictionary<string, Package>();

            foreach (string projFilePath in ProjectFiles)
            {
                var project = Project.Load(projFilePath);
                AddPackages(project, installedPackages);
            }

            SetPackageConflicts(installedPackages);

            await MetaManager.SetPackageMeta(installedPackages.Values);

            return installedPackages.Values.OrderBy(p => p.Name).ToArray();
        }

        private static void AddPackages(Project project, 
            IDictionary<string, Package> installedPackages)
        {
            foreach (var package in project.Packages)
            {
                if (! installedPackages.TryGetValue(package.PackageId, out Package installedPackage))
                {
                    installedPackage = package;
                    installedPackages[package.PackageId] = installedPackage;
                }

                installedPackage.AddProjectReference(project.Path);
            }
        }

        private static void SetPackageConflicts(IDictionary<string, Package> installedPackages)
        {
            foreach (var package in installedPackages.Values)
            {
                var multiplePackageVersions = installedPackages.Values
                    .Where(p => p.Name == package.Name)
                    .ToArray();

                bool conflictDetected = multiplePackageVersions.Length > 1;
                if (! conflictDetected) 
                {
                    continue;
                }

                int maxVersionNum = multiplePackageVersions.Max(p => p.VersionNum);
                string maxVersion = multiplePackageVersions.First(p => p.VersionNum == maxVersionNum).Version;

                // When multiple packages are found with the same name, the package with the greatest
                // installed version is not considered in conflict.  Just those with older versions.
                if (package.Version != maxVersion)
                {
                    package.HasConflict = true;
                    package.MaxVersion = maxVersion;
                }
            }
        }
    }
}
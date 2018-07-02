using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nougat.Display;
using Nougat.Meta;
using Nougat.Projects;
using Nougat.Updates;

namespace Nougat
{
    public class NougatCli
    {
        public const string DefaultSearchUrl = "https://api-v2v3search-0.nuget.org";
                
        public string ProjectDir { get; private set; }
        public string SearchUrl { get; private set; }

        private IMetaManager MetaMgr { get; set; }
        private IProjectManager ProjectMgr { get; set; }
        private IUpdateManager UpdateMgr { get; set; }

        private IDisplayManager DisplayMgr { get; set; }

        private NougatCli()
        {
        }

        public static NougatCli Create(string projectDir, ILoggerFactory loggerFactory,
             string searchUrl = null)
        {
            searchUrl = searchUrl ?? DefaultSearchUrl;

            var metaMgr = MetaManager.Create(loggerFactory, searchUrl);
            return new NougatCli {
                // Locations:
                ProjectDir = projectDir,
                SearchUrl = searchUrl,

                // Individual Managers:
                MetaMgr =  metaMgr,
                ProjectMgr = ProjectManager.Create(loggerFactory, metaMgr, projectDir),
                UpdateMgr = UpdateManager.Create(loggerFactory),
                DisplayMgr = DisplayManager.Create(Console.Out)
             };
        }

        public async Task Run()
        {
            var installedPackages = await ProjectMgr.GetInstalledPackages();

            DisplayMgr.List("INSTALLED PACKAGES", installedPackages);

            var checkUpdates = CheckForConflicts(installedPackages);
            if (checkUpdates)
            {
                var displayUpdates = CheckForUpdates(installedPackages);

                if (displayUpdates)
                {
                    installedPackages = await ProjectMgr.GetInstalledPackages();
                    DisplayMgr.List("UPDATED PACKAGES", installedPackages);
                }
            }
        }

        private bool CheckForConflicts(Package[] installedPackages)
        {
            bool hasConflicts = installedPackages.Any(p => p.HasConflict);
            if (!hasConflicts)
            {
                return true;
            }

            var conflicts = UpdateMgr.GetConflictPackageList(installedPackages);
            DisplayMgr.List("PACKAGE CONFLICTS", conflicts, displayDetails: true);
            
            Console.WriteLine();
            Console.WriteLine("Conflicting Package Versions Exist.  Enter 'Y' to Fix:");

            var input = Console.ReadLine();
            if (input == "Y")
            {
                UpdateMgr.ApplyProjectUpdate(conflicts, new string[] {});
                return true;
            }
            return false;
        }

        private bool CheckForUpdates(Package[] installedPackages)
        {
            bool hasUpdates = installedPackages.Any(p => p.HasUpdate);
            if(!hasUpdates)
            {
                return false;
            }

            var updates = UpdateMgr.GetUpdatePackageList(installedPackages);
            DisplayMgr.List("PACKAGE UPDATES", updates);

            Console.WriteLine();
            Console.WriteLine("Updated Package Versions Exist.  Enter 'Y' to Update:");

            var input = Console.ReadLine();
            if (input == "Y")
            {
                Console.WriteLine("[Enter] for all packages.");
                Console.WriteLine("...or comma separated list of package names starting with:");

                var names = Console.ReadLine().Replace(" ", "").Split(',');
                
                UpdateMgr.ApplyProjectUpdate(updates, names);
                return true;
            }
            return false;
        }
    }
}
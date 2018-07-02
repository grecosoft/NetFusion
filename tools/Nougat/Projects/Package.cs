using System.Collections.Generic;
using System.Xml;

namespace Nougat.Projects
{
    /// <summary>
    /// Repres a Nuget package.
    /// </summary>
    public class Package
    {
        private readonly XmlElement _state;

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name 
        { 
            get => _state.GetAttribute("Include");
            set => _state.Attributes["Include"].Value = value;
        }

        /// <summary>
        /// The install version of the package.
        /// </summary>
        public string Version
        {
            get => _state.GetAttribute("Version");
            set => _state.Attributes["Version"].Value = value;
        }
                
        /// <summary>
        /// The project files that reference the package.
        /// </summary>
        public List<string> Projects { get; }

        public Package(XmlElement state)
        {
            _state = state ?? throw new System.ArgumentNullException(nameof(state));
            Projects = new List<string>();
        }

        /// <summary>
        /// Value containing the name and version of a package. 
        /// </summary>
        public string PackageId => $"{Name}_{Version}";

        /// <summary>
        /// Indicates that multiple versions of a package is referenced and that
        /// this package has a lower version than the max installed version.
        /// </summary>
        public bool HasConflict { get; set; }

        /// <summary>
        /// Indicates that there is a never version of the package on the 
        /// Nuget server.
        /// </summary>
        public bool HasUpdate => LatestVersion != null && Version != LatestVersion;

        /// <summary>
        /// The latest version of the page on the Nuget server. 
        /// </summary>
        public string LatestVersion { get; set; }
        
        /// <summary>
        /// The max version of all install packages with the same name.
        /// </summary>
        public string MaxVersion { get; set; }

        /// <summary>
        /// The version number of the package. 
        /// </summary>
        public int VersionNum => int.Parse(Version.Replace(".", ""));

        /// <summary>
        /// Used to add a project file that references the package.
        /// </summary>
        /// <param name="projectFilePath">A project file containing
        /// the package.</param>
        public void AddProjectReference(string projectFilePath)
        {
            if (projectFilePath == null)
                throw new System.ArgumentNullException(nameof(projectFilePath));

            Projects.Add(projectFilePath);
        }
    }
}

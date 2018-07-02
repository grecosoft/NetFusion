using System.Linq;
using System.Xml;

namespace Nougat.Projects
{
    /// <summary>
    /// Represents a C# project loaded from a C#
    /// project file.
    /// </summary>
    public class Project
    {
        private readonly XmlDocument _state;

        /// <summary>
        /// The path of the project file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The packages contained within the project.
        /// </summary>
        public Package[] Packages { get; private set; }        

        private Project(XmlDocument state, string path)
        {
            _state = state;
            Path = path;
        }

        public static Project Load(string path)
        {
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));

            var projDoc = new XmlDocument();
            projDoc.Load(path);

            return new Project(projDoc, path)
            {
                Packages = projDoc.GetPackages().ToArray()
            }; 
        }

        /// <summary>
        /// Saved the updated project.
        /// </summary>
        public void Save()
        {
            _state.Save(Path);
        }
    }
}
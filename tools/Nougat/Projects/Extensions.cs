using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Nougat.Projects
{
    public static class ProjectExtensions
    {
        /// <summary>
        /// Returns a list of package models based on the state of an XmlDocument
        /// loaded from the C# project file.
        /// </summary>
        /// <param name="projectDoc">The project file path.</param>
        /// <returns></returns>
        public static IEnumerable<Package> GetPackages(this XmlDocument projectDoc)
        {
            if (projectDoc == null)
                throw new System.ArgumentNullException(nameof(projectDoc));

            return projectDoc.GetElementsByTagName("PackageReference")
                .OfType<XmlElement>()
                .Where(e => e.HasAttribute("Include") && e.HasAttribute("Version"))
                .Select( e => new Package(e));
        }
    }
}
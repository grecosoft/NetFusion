using Nougat.Projects;
using Nougat.Updates;

namespace Nougat.Display
{
    /// <summary>
    /// Manages how different models are rendered.
    /// </summary>
    public interface IDisplayManager
    {
        /// <summary>
        /// Displays a list of packages.
        /// </summary>
        /// <param name="caption">The caption for the displayed table.</param>
        /// <param name="packages">The list of packages to display.</param>
        void List(string caption, Package[] packages);

        /// <summary>
        /// Displays a list of packages that can be updated for a list
        /// of projects.
        /// </summary>
        /// <param name="caption">The caption for the displayed table.</param>
        /// <param name="project">The list of packages and projects in
        /// which they can be updated.</param>
        void List(string caption, ProjectUpdate project, bool displayDetails = false);
    }
}
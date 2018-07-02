namespace Nougat.Updates
{
    /// <summary>
    /// Specifies a list of packages that can be updated
    /// in an unique set of referencing projects.
    /// </summary>
    public class ProjectUpdate
    {
        public string[] ProjectFiles { get; }
        public PackageUpdate[] Packages { get; }

        public ProjectUpdate(string[] projectFiles, PackageUpdate[] packages)
        {
            ProjectFiles = projectFiles ?? throw new System.ArgumentNullException(nameof(projectFiles));
            Packages = packages ?? throw new System.ArgumentNullException(nameof(packages));
        }
    }
}
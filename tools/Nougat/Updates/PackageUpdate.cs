namespace Nougat.Updates
{
    /// <summary>
    /// Contains a package name with the current version and the
    /// version to which it can be updated.
    /// </summary>
    public class PackageUpdate
    {
        public string Name { get; }
        public string CurrentVersion { get; }
        public string UpdatedVersion { get; }

        public PackageUpdate(
            string name,
            string currentVersion, 
            string updatedVersion)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            CurrentVersion = currentVersion;
            UpdatedVersion = updatedVersion;
        }
    }
}
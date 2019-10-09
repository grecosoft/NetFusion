namespace Nougat.Meta
{
    /// <summary>
    /// DTO to pull the data from the Nuget search results.
    /// </summary>
    public class PackageSearchResult
    {
        public PackageMeta[] Data { get; set; }
    }
} 
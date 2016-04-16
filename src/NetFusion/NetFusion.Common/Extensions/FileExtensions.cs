using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetFusion.Common.Extensions
{
    public static class FileExtensions
    {
        public static IEnumerable<string> GetFileNames(this DirectoryInfo dirInfo, params string[] extensions)
        {
            return extensions.SelectMany(dirInfo.GetFiles)
                .Select(f => f.FullName)
                .Distinct();
        }
    }
}

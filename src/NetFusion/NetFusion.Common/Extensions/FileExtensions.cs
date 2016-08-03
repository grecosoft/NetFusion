using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetFusion.Common.Extensions
{
    public static class FileExtensions
    {
        public static IEnumerable<string> GetMatchingFileNames(this DirectoryInfo dirInfo, params string[] patterns)
        {
            var matchingFilesNames = new List<string>();
            
            foreach (string pattern in patterns)
            {
                var matchingFiles = dirInfo.GetFiles(pattern);
                matchingFilesNames.AddRange(matchingFiles.Select(f => f.FullName));
            }

            return matchingFilesNames.Distinct();
        }
    }
}

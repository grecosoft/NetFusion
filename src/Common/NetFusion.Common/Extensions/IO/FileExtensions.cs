using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetFusion.Common.Extensions.IO
{
    public static class FileExtensions
    {
        /// <summary>
        /// Returns all files matching a list of patterns.
        /// </summary>
        /// <param name="dirInfo">The directory to search.</param>
        /// <param name="patterns">Collection of file name patterns.</param>
        /// <returns>Files matching the specified list of patterns.</returns>
        public static IEnumerable<FileInfo> GetMatchingFiles(this DirectoryInfo dirInfo, params string[] patterns)
        {
            Check.NotNull(dirInfo, nameof(dirInfo));
            Check.NotNull(patterns, nameof(patterns));

            return patterns.Select(pattern => dirInfo.GetFiles(pattern))
                .SelectMany(f => f);
        }

        /// <summary>
        /// Returns all files matching a specified collection of file extensions.
        /// </summary>
        /// <param name="files">The list of files to filter.</param>
        /// <param name="extensions">List of file extensions.</param>
        /// <returns>Filters list of files matching specified extensions.</returns>
        public static IEnumerable<FileInfo> WithFileExtension(this IEnumerable<FileInfo> files, 
            params string[] extensions)
        {
            Check.NotNull(files, nameof(files));
            Check.NotNull(extensions, nameof(extensions));

            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }
    }
}

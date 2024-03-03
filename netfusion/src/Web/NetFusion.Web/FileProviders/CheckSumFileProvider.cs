using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace NetFusion.Web.FileProviders
{
    public class CheckSumFileProvider : IFileProvider
    {
        private readonly ConcurrentDictionary<string, CheckSumFileProviderToken> _watchers;

        public static IFileProvider? FromRelativePath(string subPath)
        {
            var executableLocation = Assembly.GetEntryAssembly()?.Location;
            if (executableLocation is null)
            {
                throw new NullReferenceException("Execution entry location could not determined.");
            }
            
            var executablePath = Path.GetDirectoryName(executableLocation);
            if (executablePath is null)
            {
                throw new NullReferenceException("Execution path could not be determined.");
            }
            
            var configPath = Path.Combine(executablePath, subPath);
            return Directory.Exists(configPath) ? new CheckSumFileProvider(configPath) : null;
        }

        public CheckSumFileProvider(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new ArgumentException("Invalid root path", nameof(rootPath));
            }

            RootPath = rootPath;
            _watchers = new ConcurrentDictionary<string, CheckSumFileProviderToken>();
        }

        public string RootPath { get; }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new PhysicalDirectoryContents(Path.Combine(RootPath, subpath));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fi = new FileInfo(Path.Combine(RootPath, subpath));
            return new PhysicalFileInfo(fi);
        }

        public IChangeToken Watch(string filter)
        {
            var watcher = _watchers.AddOrUpdate(filter, 
                addValueFactory: _ => new CheckSumFileProviderToken(RootPath, filter),
                updateValueFactory: (f, e) =>
                {
                    e.Dispose();
                    return new CheckSumFileProviderToken(RootPath, filter);
                });

            watcher.EnsureStarted();
            return watcher;
        }
    }
}
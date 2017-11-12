using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Reflection;
using System;

namespace NetFusion.Common.Extensions.Reflection
{
    /// <summary>
    /// Extensions for query an application's assemblies.
    /// </summary>
    public static class RuntimeExtensions
    {
        /// <summary>
        /// Returns all assembly names for all matching runtime libraries matching specified patterns.
        /// </summary>
        /// <param name="context">The application's dependency context to search.</param>
        /// <param name="patterns">Patterns to match against the runtime assembly names.</param>
        /// <returns>Collection of matching assemblies.</returns>
        public static AssemblyName[] ProbeForMatchingAssemblyNames(this DependencyContext context,
            params string[] patterns)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var assemblyNames = context.GetRuntimeAssemblyNames(runtimeId)
                .Where(an => MatchesPattern(an.Name, patterns))
                .ToArray();

            return assemblyNames;
        }

        private static bool MatchesPattern(string name, string[] patterns)
        {
            foreach(var pattern in patterns)
            {
                if (MatchesPattern(name, pattern))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool MatchesPattern(string name, string pattern)
        {
            var patternParts = pattern.Split('.');
            var nameParts = name.Split('.');

            if (patternParts.Length != nameParts.Length)
            {
                return false;
            }

            for (var i=0; i<patternParts.Length; i++)
            {
                var patternPart = patternParts[i].ToLower();
                var namePart = nameParts[i].ToLower();

                if (patternPart != namePart && patternPart != "*")
                {
                    return false;
                }
            }
            return true;
        }
    }
}

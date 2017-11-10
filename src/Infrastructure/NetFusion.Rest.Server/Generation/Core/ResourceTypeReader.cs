using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Generation.Core
{
    /// <summary>
    /// Loads the type-script definitions corresponding to a given controller's action method.
    /// </summary>
    public class ResourceTypeReader : IResourceTypeReader
    {
        private readonly EnvironmentSettings _environmentSettings;

        public ResourceTypeReader(EnvironmentSettings environmentSettings)
        {
            _environmentSettings = environmentSettings;
        }

        public Task<MemoryStream> ReadTypeDefinitionFiles(MethodInfo actionMethodInfo)
        {
            if (actionMethodInfo == null) throw new ArgumentNullException(nameof(actionMethodInfo));

            string generatedCodePath = _environmentSettings.GetTypeScriptPath();
            var actionMethodTypes = GetActionMethodTypes(actionMethodInfo);

            return GetTypeDefinitions(actionMethodTypes);
        }

        private async Task<MemoryStream> GetTypeDefinitions(IEnumerable<Type> resourceTypes)
        {
            string generatedCodePath = _environmentSettings.GetTypeScriptPath();

            var definitionFileNames = resourceTypes
                .Select(mt => Path.Combine(generatedCodePath, $"{mt.Name}.ts"))
                .Where(ts => File.Exists(ts))
                .Distinct();

            MemoryStream memoryStream = new MemoryStream();

            foreach (string filePath in definitionFileNames)
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task<MemoryStream> ReadTypeDefinitonFile(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            return GetTypeDefinitions(new[] { resourceType });
        }

        private IEnumerable<Type> GetActionMethodTypes(MethodInfo actionMethodInfo)
        {
            Type returnResourceType = null;
            if (actionMethodInfo.ReturnType.IsDerivedFrom<IResource>())
            {
                returnResourceType = actionMethodInfo.ReturnType;
            }

            Type paramResourceType = actionMethodInfo.GetParameters()
                .Select(p => p.ParameterType)
                .FirstOrDefault(pt => pt.IsDerivedFrom<IResource>());

            return new[] { returnResourceType, paramResourceType }.Where(rt => rt != null);
        }
    }
}

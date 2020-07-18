using System;
using NetFusion.Rest.Resources;
using TypeGen.Core.Converters;
using TypeGen.Core.SpecGeneration;
using System.Collections.Generic;

namespace NetFusion.Rest.CodeGen.Core
{
    public class ResourceGenerationSpec : GenerationSpec
    {
        private readonly IEnumerable<Type> _resourceTypes;

        public ResourceGenerationSpec(IEnumerable<Type> resourceTypes)
        {
            _resourceTypes = resourceTypes ?? throw new ArgumentNullException(nameof(resourceTypes));
        }

        public override void OnBeforeGeneration(OnBeforeGenerationArgs args)
        {
            args.GeneratorOptions.FileHeading = "";

            args.GeneratorOptions.FileNameConverters.Add(new FileNameConverter());
            args.GeneratorOptions.TypeNameConverters.Add(new TypeNameConverter());

            foreach (var resourceType in _resourceTypes)
            {
                AddClass(resourceType);
            }
        }
    }

    public class FileNameConverter : ITypeNameConverter
    {
        public string Convert(string name, Type type) => type.GetExposedResourceName();
    }

    public class TypeNameConverter : ITypeNameConverter
    {
        public string Convert(string name, Type type) => type.GetExposedResourceName();
    }
}
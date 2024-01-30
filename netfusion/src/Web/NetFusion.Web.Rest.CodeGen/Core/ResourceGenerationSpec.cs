using System;
using System.Collections.Generic;
using NetFusion.Web.Rest.Resources;
using TypeGen.Core.Converters;
using TypeGen.Core.SpecGeneration;

namespace NetFusion.Web.Rest.CodeGen.Core;

/// <summary>
/// Spec file for TypeGen Nuget used to determine the C# classes
/// for which TypeScript files should be generated.
/// </summary>
public class ResourceGenerationSpec(IEnumerable<Type> resourceTypes) : GenerationSpec
{
    private readonly IEnumerable<Type> _resourceTypes = resourceTypes ??
        throw new ArgumentNullException(nameof(resourceTypes));

    public override void OnBeforeGeneration(OnBeforeGenerationArgs args)
    {
        args.GeneratorOptions.FileHeading = string.Empty;

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
    public string Convert(string name, Type type) => type.GetResourceName();
}

public class TypeNameConverter : ITypeNameConverter
{
    public string Convert(string name, Type type) => type.GetResourceName();
}
﻿using System;
using System.Linq;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Services.Mapping.Core;

namespace NetFusion.Services.Mapping.Plugin;

/// <summary>
/// Interface implemented by a plug-in module responsible for finding all mapping strategies.
/// </summary>
public interface IMappingModule : IPluginModuleService
{
    /// <summary>
    /// A lookup keyed by the source type listing all the possible target mappings.
    /// </summary>
    ILookup<Type, TargetMap> SourceTypeMappings { get; }
}
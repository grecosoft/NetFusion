﻿using Autofac;
using NetFusion.Base.Entity;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Modules
{
    /// <summary>
    /// Plug-in module containing configurations for core domain entity
    /// implementations.
    /// </summary>
    public class DomainScriptingModule : PluginModule
    {
        // Register the Null implementation of the entity scripting service to 
        // be used by default and can be specified by other plug-ins that implement
        // script evaluation.
        public override void RegisterDefaultComponents(Autofac.ContainerBuilder builder)
        {
            builder.RegisterType<NullEntityScriptingService>()
                .As<IEntityScriptingService>()
                .SingleInstance();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["attributed:entities"] = Context.AllAppPluginTypes
                .Where(pt => pt.IsConcreteTypeDerivedFrom<IAttributedEntity>())
                .Select(ae => ae.FullName);
        }
    }
}

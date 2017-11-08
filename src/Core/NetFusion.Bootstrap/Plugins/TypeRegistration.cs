using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using NetFusion.Common;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Class used to register types with the Autofac dependency container
    /// found in a specific list of plug-ins.
    /// </summary>
    public class TypeRegistration
    {
        private readonly ContainerBuilder _builder;
        private readonly IEnumerable<PluginType> _sourceTypes; 

        internal TypeRegistration(ContainerBuilder builder, IEnumerable<PluginType> sourceTypes)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _sourceTypes = sourceTypes ?? throw new ArgumentNullException(nameof(sourceTypes));
        }

        /// <summary>
        /// List of plug-in types that can be scanned and added to the dependency 
        /// injection container.  An instance of this class is passed to each module
        /// to which it filters the types to be added to the container.
        /// </summary>
        /// <returns>List of plug-in types to be filtered.</returns>
        public IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> PluginTypes
        {
            get { return _builder.RegisterTypes(_sourceTypes.Select(pt => pt.Type).ToArray()); }
        } 
    }
}

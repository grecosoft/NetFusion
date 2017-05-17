using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Container;
using NetFusion.Utilities.Core;
using System;

namespace NetFusion.Utilities.Mapping.Configs
{
    public class MappingConfig : IContainerConfig, IKnownPluginType
    {
        /// <summary>
        /// The type implementing IAutoMapper to be used for automatically map object properties.
        /// The client application should specify and implementation delegating to a mapping
        /// library of choice.
        /// </summary>
        public Type AutoMapperType { get; private set; } = typeof(DefaultAutoMapper);

        /// <summary>
        /// Used to specify the type implementing IAutoMapper to be used.
        /// </summary>
        /// <typeparam name="T">The type used to auto map object properties.</typeparam>
        public void UseAutoMapperType<T>() where T : IAutoMapper
        {
            this.AutoMapperType = typeof(T);
        }

        // Default null instance that returns an empty object instance.
        private class DefaultAutoMapper : IAutoMapper
        {
            public TTarget Map<TTarget>(object obj) where TTarget : class
            {
                return Activator.CreateInstance<TTarget>();
            }
        }
    }
}

using System;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Configuration class used by the host application to specify an action delegate
    /// that is called and passed an instance of the Autofac container-builder.  This
    /// allows the host application to register any types and/or object instances with
    /// the builder before the container is created (i.e. logger instance).
    /// </summary>
    public class AutofacRegistrationConfig : IContainerConfig
    {
        /// <summary>
        /// Action delegate passed instance of the builder before
        /// the container is created.
        /// </summary>
        public Action<Autofac.ContainerBuilder> Build { get; set; }
    }
}

using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System;

namespace NetFusion.Testing.Logging
{
    /// <summary>
    /// Extension methods for configuring and accessing a test-logger
    /// for an application container.
    /// </summary>
    public static class ContainerLoggingExtensions
    {
        /// <summary>
        /// Configures the application container to use a test-logger.
        /// </summary>
        /// <param name="container">The container to configure.</param>
        /// <returns>Reference to the configured container.</returns>
        public static IAppContainer UseTestLogger(this IAppContainer container)
        {
            Check.NotNull(container, nameof(container));

            container.WithConfig<LoggerConfig>(config =>
            {
                var factory = new TestLoggerFactory();
                config.LogExceptions = true;
                config.UseLoggerFactory(factory);
            });

            return container;
        }

        /// <summary>
        /// Reference to the container for which the configured test-logger
        /// should be returned.
        /// </summary>
        /// <param name="container">The container configured with a test-logger.</param>
        /// <returns>Reference to the test-loger.</returns>
        public static TestLogger GetTestLogger(this IAppContainer container)
        {
            var factory = container.LoggerFactory as TestLoggerFactory;

            if (factory == null)
            {
                throw new InvalidOperationException(
                    "The test-logger has not been configured.");
            }

            return factory.Logger;
        }
    }
}

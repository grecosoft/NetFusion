using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Validation;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Class delegated to by the CompositeApp that logs the implementations registered for
    /// overall core services used by NetFusion.  The CompositeContainerBuilder registers
    /// service defaults or null-implementations that can be overriden by the host application
    /// by a plugin-module or during bootstrapping the composite-application when calling
    /// the Compose method on ICompositeContainerBuilder.
    /// </summary>
    public class CoreServicesLog
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;
        private readonly ICompositeAppBuilder _builder;
        
        public CoreServicesLog(ILogger logger, 
            IServiceProvider services,
            ICompositeAppBuilder builder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void Log()
        {
            LogContainerConfigs();
            LogSerializationManager();
            LogScriptingService();
        }

        private void LogContainerConfigs()
        {
            var validationConfig = _builder.GetContainerConfig<ValidationConfig>();
            
            _logger.LogDebug("Registered Object Validator: {validator}",
                validationConfig.ValidatorType.AssemblyQualifiedName);
        }
        
        private void LogSerializationManager()
        {
            var serializerMgr = _services.GetService<ISerializationManager>();
            if (serializerMgr == null)
            {
                // This should never happen since the CompositeAppBuilder adds a default implementation:
                _logger.LogWarning(
                    "Serializer Manager Service not Registered.");
            }
            else
            {
                _logger.LogDebug("Registered Serializer Manager: {serializer}", 
                    serializerMgr.GetType().AssemblyQualifiedName);
                
                var serializerDetails = new {
                    SerializationManager = serializerMgr.GetType().FullName,
                    Serializers = serializerMgr.Serializers.Select(s => new {
                        s.ContentType,
                        SerializerType = s.GetType().FullName
                    }).ToArray()
                };
                
                _logger.LogTraceDetails("Available Serializers", serializerDetails);
            }
        }
        
        private void LogScriptingService()
        {
            var scriptingSrv = _services.GetService<IEntityScriptingService>();
            if (scriptingSrv == null)
            {
                // This should never happen since the CompositeAppBuilder adds a default implementation:
                _logger.LogWarning(
                    "Scripting Service not Registered.");
            }
            else
            {
                _logger.LogDebug("Registered Scripting Service: {serializer}", 
                    scriptingSrv.GetType().AssemblyQualifiedName);
            }
        }
    }
}
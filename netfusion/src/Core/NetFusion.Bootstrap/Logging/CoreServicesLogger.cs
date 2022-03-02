using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Class delegated to by the CompositeApp that logs the implementations registered for
    /// overall core services used by NetFusion.  The CompositeContainerBuilder registers
    /// service defaults or null-implementations that can be overridden by the host application
    /// by a plugin-module or during bootstrapping the composite-application when calling
    /// the Compose method on ICompositeContainerBuilder.
    /// </summary>
    public static class CoreServicesLogger
    {
        public static IEnumerable<LogMessage> Log(IServiceProvider services)
        {
            yield return LogSerializationManager(services);
            yield return LogScriptingService(services);
        }

        private static LogMessage LogSerializationManager(IServiceProvider services)
        {
            var serializerMgr = services.GetService<ISerializationManager>();
            if (serializerMgr == null)
            {
                return LogMessage.For(LogLevel.Warning, "Serializer Manager not Registered");
            }

            var logMessage = LogMessage.For(LogLevel.Information, "Serialization Manager Registered");
            logMessage.WithProperties(new LogProperty {
                Name = "ManagerType", 
                Value = serializerMgr.GetType().AssemblyQualifiedName
            });
            
            var contentTypes = new {
                Serializers = serializerMgr.Serializers.Select(s => new {
                    s.ContentType,
                    SerializerType = s.GetType().FullName
                }).ToArray()
            };

            logMessage.WithProperties(
                new LogProperty { Name = "ContentTypes", Value = contentTypes });

            return logMessage;
        }
        
        private static LogMessage LogScriptingService(IServiceProvider services)
        {
            var scriptingSrv = services.GetService<IEntityScriptingService>();
            if (scriptingSrv == null)
            {
                return LogMessage.For(LogLevel.Warning, "Scripting Service not Registered");
            }

            return LogMessage.For(LogLevel.Information, "Scripting Service Registered")
                .WithProperties(new LogProperty {
                    Name = "Service",
                    Value = scriptingSrv.GetType().AssemblyQualifiedName
                });
        }
    }
}
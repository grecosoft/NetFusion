using NetFusion.Bootstrap.Container;
using NetFusion.Web.Mvc.Composite.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NetFusion.Web.Mvc.Composite.Core
{
    /// <summary>
    /// Returns information about the composite structure of the application
    /// based on the information logged during the bootstrap process.
    /// </summary>
    public class CompositeService : ICompositeService
    {
        private readonly IDictionary<string, object> _compositeLog;

        public CompositeService(ICompositeApp compositeApp)
        {
            _compositeLog = compositeApp.Log;
        }

        public CompositeStructure GetStructure()
        {
            if (_compositeLog == null)
            {
                // This should never be the case.
                return null;
            }

            return CompositeStructure.FromLog(_compositeLog);
        }

        public PluginDetails GetPluginDetails(string pluginId)
        {
            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new ArgumentException("Plugin identity value not specified.", nameof(pluginId));
            }

            var corePlugins = _compositeLog["Plugins:Core"] as IDictionary;
            var appPlugins = _compositeLog["Plugins:Application"] as IDictionary;

            IDictionary<string, object> hostPlugin = (IDictionary<string, object>)_compositeLog["Plugin:Host"];

            object plugInProperties = null;
            if (hostPlugin["Plugin:Id"].ToString() == pluginId)
            {
                plugInProperties = hostPlugin;
            }

            plugInProperties = plugInProperties ?? corePlugins[pluginId] ?? appPlugins[pluginId];
            if (plugInProperties != null)
            {
                return PluginDetails.FromLog((IDictionary)plugInProperties);
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Represents a complete URL link with all route parameters replaced with
    /// corresponding values from the associated resource's state.  This class
    /// contains the information passed to the IUrlHelper used to generate the
    /// link based on the resource's state.
    /// </summary>
    public class ControllerActionLink : ResourceLink
    {
        /// <summary>
        /// The name of the controller used to determine the URL.
        /// </summary>
        public string Controller { get; internal set; }

        /// <summary>
        /// The name of the controller's action used to determine the URL.
        /// </summary>
        public string Action { get; internal set; }
        
        public MethodInfo ActionMethodInfo { get; internal set; }

        /// <summary>
        /// The controller's action route parameters and the corresponding
        /// resource state properties from which they are populated.
        /// </summary>
        public IReadOnlyCollection<RouteParameter> RouteParameters { get; private set; }

        public ControllerActionLink()    
        {
            RouteParameters = _routeValues.AsReadOnly();            
        }

        private readonly List<RouteParameter> _routeValues = new List<RouteParameter>();

        /// <summary>
        /// Adds a mapping indicating the resource's state properties corresponding to an action parameter.
        /// </summary>
        /// <param name="routeParam">Mapping between action parameter and corresponding resource's state property.</param>
        public void AddRouteValue(RouteParameter routeParam)
        {
            if (routeParam == null) throw new ArgumentNullException(nameof(routeParam),
                "Route parameter cannot be null.");

            if (_routeValues.Any(rv => rv.ActionParamName == routeParam.ActionParamName))
            {
                throw new ArgumentException($"The action parameter named: {routeParam.ActionParamName} has already been added.");
            }

            _routeValues.Add(routeParam);
        }
    }
}

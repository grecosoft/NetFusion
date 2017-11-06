using NetFusion.Common.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Represents a complete link with all route parameters replaced with
    /// corresponding values from the associated resource's state.
    /// </summary>
    public class ActionUrlLink : ActionLink
    {
        /// <summary>
        /// The name of the controller used to determine the URL.
        /// </summary>
        public string Controller { get; internal set; }

        /// <summary>
        /// The name of the controller's action used to determine the URL.
        /// </summary>
        public string Action { get; internal set; }

        /// <summary>
        /// The route values to be used for the parameters contained in the route.
        /// </summary>
        public IReadOnlyCollection<ActionParamValue> RouteValues { get; private set; }

        public ActionUrlLink()
        {
            RouteValues = _routeValues.AsReadOnly();            
        }

        public ActionUrlLink(ActionParamValue actionParamValues) 
        {
            if (actionParamValues == null)
                throw new ArgumentNullException(nameof(actionParamValues), "Action parameter values not specified.");

            _routeValues = new List<ActionParamValue> { actionParamValues };
            RouteValues = _routeValues.AsReadOnly();
        }

        private void SetRouteValues(IEnumerable<ActionParamValue> actionPramValues)
        {
            _routeValues = actionPramValues.ToList();
            RouteValues = _routeValues.AsReadOnly(); 
        }

        private List<ActionParamValue> _routeValues = new List<ActionParamValue>();

        /// <summary>
        /// Adds a mapping indicating the resources's property corresponding to an action parameter.
        /// </summary>
        /// <param name="actionParam">Mapping between action parameter and corresponding resource property.</param>
        public void AddRouteValue(ActionParamValue actionParam)
        {
            if (actionParam == null)
                throw new ArgumentNullException(nameof(actionParam), "Route parameter value not specified.");

            if (_routeValues.Any(rv => rv.ActionParamName == actionParam.ActionParamName))
            {
                throw new ArgumentException($"The action parameter named: {actionParam.ActionParamName} has already been added.");
            }

            _routeValues.Add(actionParam);
        }

        // The link only can be applied to another resource type if it has all of the
        // route values defined on the original resource on witch the route is based.
        internal override bool CanBeAppliedTo(Type resourceType)
        {
            var resourceProps = resourceType.GetProperties();
            var linkResourceProps = _routeValues.Select(rv => rv.ResourcePropInfo);

            return linkResourceProps.All(lrp => resourceProps.Any(
                    rp => rp.Name == lrp.Name && rp.PropertyType == lrp.PropertyType));
        }

        // Populate new instance of the link related with new resource type.
        internal override void CopyTo<TNewResourceType>(ActionLink actionLink)
        {
            base.CopyTo<TNewResourceType>(actionLink);

            var actionUrlLink = (ActionUrlLink)actionLink;
            actionUrlLink.Controller = Controller;
            actionUrlLink.Action = Action;
            actionUrlLink.Methods = Methods;

            var newResourceRouteValues = RouteValues.Select(rv => rv.CreateCopyFor<TNewResourceType>());
            actionUrlLink.SetRouteValues(newResourceRouteValues);
        }

        // Create new instance of the link for the new resource type.
        internal override ActionLink CreateCopyFor<TNewResourceType>()
        {
            var newResourceLink = new ActionUrlLink();

            CopyTo<TNewResourceType>(newResourceLink);
            return newResourceLink;
        }
    }
}

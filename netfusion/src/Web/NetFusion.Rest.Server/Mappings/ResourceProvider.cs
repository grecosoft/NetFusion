using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Actions;
using NetFusion.Web.Mvc.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Base provider that processes IResource based resources and applies
    /// REST links to the resource.
    /// </summary>
    public abstract class ResourceProvider : IResourceProvider
    {
        public virtual void ApplyResourceMeta(ResourceContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var linkedResource = context.Resource as ILinkedResource;
            if (linkedResource == null || context.Meta.Links.Count == 0)
            {
                return;
            }

            linkedResource.Links = linkedResource.Links ?? new Dictionary<string, Link>();

            foreach (ActionLink actionLink in context.Meta.Links)
            {
                // Note:  Common .NET trick this allows the method to be called based on
                // the runtime type and not the compile-time type.
                SetLinkUrl(context, (dynamic)actionLink);
            }
        }

        // Called for action link consisting of a hard-coded URL value.  
        private Link SetLinkUrl(ResourceContext context, ActionLink actionLink)
        {
            var link = new Link
            {
                Href = actionLink.Href,
                Methods = actionLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, actionLink, link);
            return link;
        }

        // Called for action link consisting of a string interpolation format based on resource properties.
        private Link SetLinkUrl(ResourceContext context, ActionResourceLink actionLink)
        {
            var link = new Link
            {
                Href = actionLink.FormatUrl(context.Resource),
                Methods = actionLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, actionLink, link);
            return link;
        }

        // Called for action link containing information based on an expression, specified at compile time, selecting a
        // controller's action method.  The expression also specifies which resource properties should be used for
        // the action's route parameters. 
        private Link SetLinkUrl(ResourceContext context, ActionUrlLink actionLink)
        {
            string controllerSuffix = context.RestModule.GetControllerSuffix();
            string controllerName = actionLink.Controller.Replace(controllerSuffix, string.Empty);

            var routeValues = GetModelRouteValues(context, actionLink);
            var link = new Link
            {
                // Delegate to ASP.NET Core to get the URL corresponding to the route-values.
                Href = context.UrlHelper.Action(actionLink.Action, controllerName, routeValues),
                Templated = false,
                Methods = actionLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, actionLink, link);
            return link;
        }

        // For each controller action argument execute the cached expression on the resource
        // to get the corresponding resource property value.
        private Dictionary<string, object> GetModelRouteValues(ResourceContext context, ActionUrlLink halLink)
        {
            var modelRouteValues = new Dictionary<string, object>(halLink.RouteValues.Count);
            foreach (ActionParamValue actionParam in halLink.RouteValues)
            {
                modelRouteValues[actionParam.ActionParamName] = actionParam.GetModelPropValue(context.Resource);
            }
            return modelRouteValues;
        }

        // Called for action link containing information based on an expression, specified at compile time, selecting a
        // controller's action method for which its corresponding URL template is used.  For this type of link, the 
        // consumer is responsible for specifying the route parameter values.
        private Link SetLinkUrl(ResourceContext context, ActionTemplateLink actionLink)
        {
            var apiAction = context.ApiMetadata.GetApiAction(
                actionLink.GroupTemplateName, 
                actionLink.ActionTemplateName);

            var link = new Link
            {
                Href = apiAction.RelativePath,
                Methods = new[] { apiAction.HttpMethod }
            };

            MarkOptionalParams(apiAction, link);
            UpdateLinkDescriptorsAndResource(context, actionLink, link);

            return link;
        }

        private void UpdateLinkDescriptorsAndResource(ResourceContext context, ActionLink actionLink, Link link)
        {
            SetLinkTemplatedIndicator(link);
            SetLinkOptionalDescriptors(actionLink, link);
            SetLinkBasedDescriptors(context, actionLink, link);

            // Only generate links with relation-names are added to the resource.
            if (actionLink.RelationName != null)
            {
                var linkedResource = (ILinkedResource)context.Resource;
                linkedResource.Links.Add(actionLink.RelationName, link);
            }
        }

        private static void SetLinkTemplatedIndicator(Link link)
        {
            // Value has already been determined based on URL link generation.
            if (link.Templated != null)
            {
                return;
            }

            link.Templated = link.Href.Contains("{") && link.Href.Contains("}");
        }

        private void SetLinkOptionalDescriptors(ActionLink actionLink, Link link)
        {
            link.HrefLang = actionLink.HrefLang;
            link.Name = actionLink.Name;
            link.Title = actionLink.Title;
            link.Type = actionLink.Type;
        }

        private void SetLinkBasedDescriptors(ResourceContext context, ActionLink actionLink, Link link)
        {
            if (actionLink.Deprecation != null)
            {
                link.Deprecation = SetLinkUrl(context, (dynamic)actionLink.Deprecation);
            }

            if (actionLink.Profile != null)
            {
                link.Profile = SetLinkUrl(context, (dynamic)actionLink.Profile);
            }
        }
           
        private void MarkOptionalParams(ApiActionMeta actionMeta, Link link)
        {
            foreach(ApiParameterMeta paramMeta in actionMeta.Parameters.Where(p => p.IsOptional))
            {
                link.Href = link.Href.Replace(
                    "{" + paramMeta.ParameterName + "}", 
                    "{?" + paramMeta.ParameterName + "}");
            }
        }
    }
}

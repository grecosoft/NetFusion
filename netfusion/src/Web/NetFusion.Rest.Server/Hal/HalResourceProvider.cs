using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources;
using NetFusion.Web.Mvc.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Linking;
using NetFusion.Rest.Server.Mappings;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// HAL provider that processes IHalResource based resources and applies
    /// REST links to the resource.
    /// </summary>
    public class HalResourceProvider : IResourceProvider
    {
        /// <summary>
        /// If the resource type being returned supports the IHalResource interface,
        /// the link metadata is used to generate resource specific URLs. 
        /// </summary>
        /// <param name="context">The context for the current response.</param>
        public virtual void ApplyResourceMeta(ResourceContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var linkedResource = context.Resource as IHalResource;
            if (linkedResource == null || context.Meta.Links.Count == 0)
            {
                return;
            }

            linkedResource.Links ??= new Dictionary<string, Link>();

            // For each associated link metadata, generate the corresponding URL and
            // associate it with the resource.
            foreach (ResourceLink resourceLink in context.Meta.Links)
            {
                // Note:  Common .NET trick this allows the method to be called based on
                // the runtime type and not the compile-time type.
                SetLinkUrl(context, (dynamic)resourceLink);
            }
        }

        // Called for action link consisting of a hard-coded URL value.  
        private Link SetLinkUrl(ResourceContext context, ResourceLink resourceLink)
        {
            var link = new Link
            {
                Href = resourceLink.Href,
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
            return link;
        }

        // Called for action link consisting of a string interpolation format based on resource properties.
        private Link SetLinkUrl(ResourceContext context, InterpolatedLink resourceLink)
        {
            var link = new Link
            {
                Href = resourceLink.FormatUrl(context.Resource),
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
            return link;
        }

        // Called for action link containing information based on an expression, specified at compile time, selecting a
        // controller's action method.  The expression also specifies which resource properties should be used for
        // the action's route parameters. 
        private Link SetLinkUrl(ResourceContext context, ControllerActionLink resourceLink)
        {
            string controllerName = resourceLink.Controller.Replace("Controller", string.Empty);
            var routeValues = GetResourceRouteValues(context, resourceLink);
            
            var link = new Link
            {
                // Delegate to ASP.NET Core to get the URL corresponding to the route-values.
                Href = context.UrlHelper.Action(resourceLink.Action, controllerName, routeValues),
                Templated = false,
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
            return link;
        }

        // For each controller action argument execute the cached expression on the resource
        // to get the corresponding resource property value for that route parameter.
        private static Dictionary<string, object> GetResourceRouteValues(ResourceContext context, ControllerActionLink resourceLink)
        {
            var modelRouteValues = new Dictionary<string, object>(resourceLink.RouteParameters.Count);
            foreach (RouteParameter routeParam in resourceLink.RouteParameters)
            {
                modelRouteValues[routeParam.ActionParamName] = routeParam.GetPropValue(context.Resource);
            }
            return modelRouteValues;
        }

        // Called for action link containing information based on an expression, specified at compile time, selecting a
        // controller's action method for which its corresponding URL template is used.  For this type of link, the 
        // consumer is responsible for specifying the route parameter values.
        private static Link SetLinkUrl(ResourceContext context, TemplateUrlLink resourceLink)
        {
            var apiAction = context.ApiMetadata.GetApiAction(
                resourceLink.GroupTemplateName, 
                resourceLink.ActionTemplateName);

            var link = new Link
            {
                Href = apiAction.RelativePath,
                Methods = new[] { apiAction.HttpMethod }
            };

            MarkOptionalParams(apiAction, link);
            UpdateLinkDescriptorsAndResource(context, resourceLink, link);

            return link;
        }

        private static void UpdateLinkDescriptorsAndResource(ResourceContext context, ResourceLink resourceLink, Link link)
        {
            SetLinkTemplateIndicator(link);
            SetLinkOptionalDescriptors(resourceLink, link);

            var linkedResource = (IHalResource)context.Resource;
            linkedResource.Links.Add(resourceLink.RelationName, link);
        }

        private static void SetLinkTemplateIndicator(Link link)
        {
            // Value has already been determined based on URL link generation.
            if (link.Templated != null)
            {
                return;
            }

            link.Templated = link.Href.Contains("{") && link.Href.Contains("}");
        }

        private static void SetLinkOptionalDescriptors(ResourceLink resourceLink, Link link)
        {
            link.HrefLang = resourceLink.HrefLang;
            link.Name = resourceLink.Name;
            link.Title = resourceLink.Title;
            link.Type = resourceLink.Type;
        }

        private static void MarkOptionalParams(ApiActionMeta actionMeta, Link link)
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

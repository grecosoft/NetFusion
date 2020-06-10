using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Linking;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Web.Mvc.Metadata.Core;

namespace NetFusion.Rest.Server.Hal.Core
{
    /// <summary>
    /// HAL provider that processes IHalResource based resources and applies
    /// REST links to the resource.
    /// </summary>
    public class HalResourceProvider : IResourceProvider
    {
        /// <summary>
        /// If the resource type being returned is of type HalResource the,
        /// link metadata is used to generate resource specific URLs. 
        /// </summary>
        /// <param name="context">The context for the current response.</param>
        public virtual void ApplyResourceMeta(ResourceContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if ( !(context.Resource is HalResource halResource) || context.Meta.Links.Count == 0)
            {
                return;
            }

            halResource.Links ??= new Dictionary<string, Link>();

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
        private static void SetLinkUrl(ResourceContext context, ResourceLink resourceLink)
        {
            var link = new Link
            {
                Href = resourceLink.Href,
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        // Called for action link consisting of a string interpolation format based on resource properties.
        private static void SetLinkUrl(ResourceContext context, InterpolatedLink resourceLink)
        {
            var link = new Link
            {
                Href = resourceLink.FormatUrl(context.Model),
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        // Called for action link containing information based on an expression, specified at compile time,
        // selecting a controller's action method.  The expression also specifies which model properties used
        // for the action's route parameters. 
        private static void SetLinkUrl(ResourceContext context, ControllerActionLink resourceLink)
        {
            ApiActionMeta actionDescriptor = context.ApiMetadata.GetActionDescriptor(resourceLink.ActionMethodInfo);

            string controllerName = actionDescriptor.ControllerName;
            var routeValues = GetModelRouteValues(context, resourceLink);
            
            var link = new Link
            {
                // Delegate to ASP.NET Core to get the URL corresponding to the route-values.
                // This is known as the out-going URL in ASP.NET.
                Href = context.UrlHelper.Action(resourceLink.Action, controllerName, routeValues),
                Templated = false,
                Methods = resourceLink.Methods.ToArray()
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        // For each controller action argument executes the cached expression on the model
        // to get the corresponding model property value for that route parameter.
        private static Dictionary<string, object> GetModelRouteValues(ResourceContext context, ControllerActionLink resourceLink)
        {
            var modelRouteValues = new Dictionary<string, object>(resourceLink.RouteParameters.Count);
            foreach (RouteParameter routeParam in resourceLink.RouteParameters)
            {
                modelRouteValues[routeParam.ActionParamName] = routeParam.GetSourcePropValue(context.Model);
            }
            return modelRouteValues;
        }

        // Called for action link containing information based on an expression, specified at compile time, selecting a
        // controller's action method for which its corresponding URL template is used.  For this type of link, the 
        // consumer is responsible for specifying the route parameter values.
        private static void SetLinkUrl(ResourceContext context, TemplateUrlLink resourceLink)
        {
            ApiActionMeta apiMeta = context.ApiMetadata.GetActionDescriptor(resourceLink.ActionMethodInfo);
            
            var link = new Link
            {
                Href = apiMeta.RelativePath,
                Methods = new[] { apiMeta.HttpMethod }
            };

            MarkOptionalParams(apiMeta, link);
            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        private static void UpdateLinkDescriptorsAndResource(ResourceContext context, ResourceLink resourceLink, Link link)
        {
            if (string.IsNullOrWhiteSpace(link.Href))
            {    
                context.Logger.LogError(
                    "The Href value for the link '{named}' for resource '{resourceName}' was not set.", 
                    resourceLink.RelationName, (context.Model ?? context.Resource).GetType());
                return;
            }
            
            SetLinkTemplateIndicator(link);
            SetLinkOptionalDescriptors(resourceLink, link);

            var halResource = (HalResource)context.Resource;
            halResource.Links.Add(resourceLink.RelationName, link);
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

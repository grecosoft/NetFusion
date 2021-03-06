﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Linking;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Server.Hal.Core
{
    /// <summary>
    /// HAL provider that processes HalResource based resources and applies
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

            if (context.Resource is not HalResource halResource || context.Meta.Links.Count == 0)
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
                Method = resourceLink.Method
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        // Called for action link consisting of a string interpolation format based on resource properties.
        private static void SetLinkUrl(ResourceContext context, InterpolatedLink resourceLink)
        {
            var link = new Link
            {
                Href = resourceLink.FormatUrl(context.Model),
                Method = resourceLink.Method
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        }

        // Called for action link containing information based on an expression, specified at compile time,
        // selecting a controller's action method.  The expression also specifies which model properties used
        // for the action's route parameters. 
        private static void SetLinkUrl(ResourceContext context, ControllerActionLink resourceLink)
        {
            ApiActionMeta actionMeta = context.ApiMetadata.GetActionMeta(resourceLink.ActionMethodInfo);
            
            var routeValues = GetModelRouteValues(context, resourceLink);
            
            var link = new Link
            {
                // Delegate to ASP.NET Core to get the URL corresponding to the route-values.
                // This is known as the out-going URL in ASP.NET.
                Href = context.UrlHelper.Action(
                    actionMeta.ActionName, 
                    actionMeta.ControllerName, 
                    routeValues),
                
                Templated = false,
                Method = actionMeta.HttpMethod
            };

            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
            SetRouteForDocQuery(context, actionMeta, link);
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
            ApiActionMeta actionMeta = context.ApiMetadata.GetActionMeta(resourceLink.ActionMethodInfo);
            
            var link = new Link
            {
                Href = actionMeta.RelativePath,
                Method = actionMeta.HttpMethod
            };

            MarkOptionalParams(actionMeta, link);
            UpdateLinkDescriptorsAndResource(context, resourceLink, link);
            SetRouteForDocQuery(context, actionMeta, link);
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

            // Associate the link with the resource:
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
            link.Title = resourceLink.Title;
            link.Type = resourceLink.Type;
        }

        private static void MarkOptionalParams(ApiActionMeta actionMeta, Link link)
        {
            foreach(ApiParameterMeta paramMeta in actionMeta.RouteParameters.Where(p => p.IsOptional))
            {
                link.Href = link.Href.Replace(
                    "{" + paramMeta.ParameterName + "}", 
                    "{?" + paramMeta.ParameterName + "}");
            }
        }

        // Indicates if the request has a header value indicating that the URL template
        // should be added to the returned Links.  The URL template is used to query
        // the associated documentation for the corresponding controller action.
        private static void SetRouteForDocQuery(ResourceContext context, ApiActionMeta actionMeta, Link link)
        {
            var headers = context.HttpContext.Request.Headers;

            if (headers.TryGetValue("include-url-for-doc-query", out var values) && values.Contains("yes"))
            {
                link.DocQuery = actionMeta.RelativePath;
            }
        }
    }
}

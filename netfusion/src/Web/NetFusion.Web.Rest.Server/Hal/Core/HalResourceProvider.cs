using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Resources;
using NetFusion.Web.Rest.Server.Linking;
using NetFusion.Web.Rest.Server.Mappings;

namespace NetFusion.Web.Rest.Server.Hal.Core;

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
    public void ApplyResourceMeta(ResourceContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (context.Meta == null) throw new NullReferenceException("Context Metadata not set");
            
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
        if (resourceLink.Href == null)
        {
            throw new InvalidOperationException(
                $"Href not set for relation name: {resourceLink.RelationName}");
        }
            
        var link = new Link(resourceLink.Method, resourceLink.Href);
        UpdateLinkDescriptorsAndResource(context, resourceLink, link);
    }

    // Called for action link consisting of a string interpolation format based on model properties.
    private static void SetLinkUrl(ResourceContext context, InterpolatedLink resourceLink)
    {
        if (context.Model == null) throw new NullReferenceException("Model Required on ResourceContext");
        
        var link = new Link(method: resourceLink.Method, href: resourceLink.FormatUrl(context.Model));
        UpdateLinkDescriptorsAndResource(context, resourceLink, link);
    }

    // Called for action link containing information based on an expression, specified at compile time,
    // selecting a controller's action method.  The expression also specifies which model properties used
    // for the action's route parameters. 
    private static void SetLinkUrl(ResourceContext context, ControllerActionLink resourceLink)
    {
        ApiActionMeta actionMeta = context.ApiMetadata.GetActionMeta(resourceLink.ActionMethodInfo);
            
        var routeValues = GetModelRouteValues(context, resourceLink);
            
        // Delegate to ASP.NET Core to get the URL corresponding to the route-values.
        // This is known as the out-going URL in ASP.NET.
        string href = context.UrlHelper.Action(
            actionMeta.ActionName,
            actionMeta.ControllerName,
            routeValues) ?? string.Empty;
                
        var link = new Link(actionMeta.HttpMethod, href)
        {
            Templated = false
        };

        UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        SetRouteForDocQuery(context, actionMeta, link);
    }

    // For each controller action argument executes the cached expression on the model
    // to get the corresponding model property value for that route parameter.
    private static Dictionary<string, object> GetModelRouteValues(ResourceContext context, 
        ControllerActionLink resourceLink)
    {
        if (context.Model == null) throw new NullReferenceException("Model Required on ResourceContext");
        
        var modelRouteValues = new Dictionary<string, object>(resourceLink.RouteParameters.Count);
        foreach (RouteParameter routeParam in resourceLink.RouteParameters)
        {
            object? propValue = routeParam.GetSourcePropValue(context.Model);
            if (propValue != null)
            {
                modelRouteValues[routeParam.ActionParamName] = propValue;
            }
        }
        return modelRouteValues;
    }

    // Called for action link containing information based on an expression, specified at compile time, selecting a
    // controller's action method for which its corresponding URL template is used.  For this type of link, the 
    // Api consumer is responsible for specifying the route parameter values.
    private static void SetLinkUrl(ResourceContext context, TemplateUrlLink resourceLink)
    {
        ApiActionMeta actionMeta = context.ApiMetadata.GetActionMeta(resourceLink.ActionMethodInfo);
            
        var link = new Link(actionMeta.HttpMethod, href: actionMeta.RelativePath);

        MarkOptionalParams(actionMeta, link);
        UpdateLinkDescriptorsAndResource(context, resourceLink, link);
        SetRouteForDocQuery(context, actionMeta, link);
    }

    private static void UpdateLinkDescriptorsAndResource(ResourceContext context, ResourceLink resourceLink, Link link)
    {
        SetLinkTemplateIndicator(link);
        SetLinkOptionalDescriptors(resourceLink, link);

        // Associate the link with the resource:
        var halResource = (HalResource)context.Resource;
        halResource.Links ??= new Dictionary<string, Link>();
        halResource.Links.Add(resourceLink.RelationName, link);
    }

    private static void SetLinkTemplateIndicator(Link link)
    {
        // Value has already been determined based on URL link generation.
        if (link.Templated != null)
        {
            return;
        }

        link.Templated = link.Href.Contains('{') && link.Href.Contains('}');
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
using System;
using System.Net.Http;
using NetFusion.Web.Rest.Server.Linking;

namespace NetFusion.Web.Rest.Server.Meta;

/// <summary>
/// Instance of class used to specify required and optional link metadata properties.
/// </summary>
/// <typeparam name="TSource">The source type associated with the metadata.</typeparam>
public class LinkDescriptor<TSource>(ResourceLink resourceLink)
    where TSource : class
{
    private readonly ResourceLink _resourceLink = resourceLink ?? throw new ArgumentNullException(nameof(resourceLink));

    //---------------- REQUIRED LINK PROPERTIES SET BY LINK METADATA METHODS ----------------------
        
    internal LinkDescriptor<TSource> SetMethod(HttpMethod method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method), 
            "HTTP Method cannot be null.");

        _resourceLink.Method = method.Method;

        return this;
    }

    internal LinkDescriptor<TSource> SetHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            throw new ArgumentException("Href Value not specified.", nameof(href));

        _resourceLink.Href = href;
        return this;
    }

    //---------------- OPTIONAL LINK PROPERTIES SET BY RESOURCE MAPPINGS ----------------------

    public LinkDescriptor<TSource> SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title Value not specified.", nameof(title));

        _resourceLink.Title = title;
        return this;
    }

    public LinkDescriptor<TSource> SetType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type Value not specified.", nameof(type));

        _resourceLink.Type = type;
        return this;
    }

    public LinkDescriptor<TSource> SetHrefLang(string hrefLang)
    {
        if (string.IsNullOrWhiteSpace(hrefLang))
            throw new ArgumentException("HrefLang Value not specified.", nameof(hrefLang));

        _resourceLink.HrefLang = hrefLang;
        return this;
    }
}
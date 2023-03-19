using System.Net.Http;

namespace NetFusion.Web.Rest.Resources;

/// <summary>
/// Defines a link that can be associated with a resource.
/// </summary>
public class Link
{
    /// <summary>
    /// The hypertext reference to the associated resource.
    /// </summary>
    public string Href { get; set; }
        
    /// <summary>
    /// Indicates the method that can be used to call the HREF.
    /// </summary>
    public string Method { get; set; }
    
    public Link(string method, string href)
    {
        Href = href;
        Method = method;
    }
    
    public Link() : this(string.Empty, HttpMethod.Get.Method)
    {
       
    }
    
    public Link(string href) : this(href, HttpMethod.Get.Method)
    {

    }

    /// <summary>
    /// Optionally set based on if the caller specified the header to set the
    /// underlying action route used to query its corresponding documentation.
    /// </summary>
    public string? DocQuery { get; set; }

    /// <summary>
    /// Its value is a string and is intended for indicating the language of
    /// the target resource(as defined by [RFC5988]) (Optional).
    /// </summary>
    public string? HrefLang { get; set; }

    /// <summary>
    /// Indicates if the HREF value is a template (Optional).
    /// </summary>
    public bool? Templated { get; set; }

    /// <summary>
    /// Its value is a string and is intended for labeling the link with a
    /// human-readable identifier(as defined by [RFC5988]) (Optional).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Its value is a string used as a hint to indicate the media type
    /// expected when dereferencing the target resource (Optional).
    /// </summary>
    public string? Type { get; set; }
}
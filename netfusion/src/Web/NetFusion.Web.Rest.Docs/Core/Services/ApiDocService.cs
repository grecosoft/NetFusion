using System;
using System.Reflection;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Core.Services;

/// <summary>
/// Main entry service responsible for returning documentation for controller's action methods.
/// </summary>
public class ApiDocService(
    IApiMetadataService apiMetadata,
    IDocBuilder docBuilder)
    : IApiDocService
{
    private readonly IApiMetadataService _apiMetadata = apiMetadata ??
        throw new ArgumentNullException(nameof(apiMetadata));
    
    private readonly IDocBuilder _docBuilder = docBuilder ?? 
        throw new ArgumentNullException(nameof(docBuilder));

    public bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc)
    {
        ArgumentNullException.ThrowIfNull(actionMethodInfo);

        actionDoc = null;      

        // Determines if there is metadata associated with the action method.
        // If metadata is found, build an action document model from the metadata.
        if (_apiMetadata.TryGetActionMeta(actionMethodInfo, out ApiActionMeta actionMeta))
        {
            actionDoc = _docBuilder.BuildActionDoc(actionMeta);
        }

        return actionDoc != null;
    }

    public bool TryGetActionDoc(string httpMethod, string relativePath, out ApiActionDoc actionDoc)
    {
        if (string.IsNullOrWhiteSpace(httpMethod))
            throw new ArgumentException("Http Method must be Specified.", nameof(httpMethod));
            
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative Path must be Specified.", nameof(httpMethod));
            
        actionDoc = null;

        // Determines if there is metadata associated with the action method.
        // If metadata is found, builds an action document model from the metadata.
        if (_apiMetadata.TryGetActionMeta(httpMethod, relativePath, out ApiActionMeta actionMeta))
        {
            actionDoc = _docBuilder.BuildActionDoc(actionMeta);
        }

        return actionDoc != null;
    }
}
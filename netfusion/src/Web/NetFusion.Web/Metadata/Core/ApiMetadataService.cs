using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace NetFusion.Web.Metadata.Core;

/// <summary>
/// Service returning metadata for an MVC applications defined controllers and routes.
/// </summary>
public class ApiMetadataService : IApiMetadataService
{
    private readonly ApiActionMeta[] _apiActionMeta;

    public ApiMetadataService(
        IApiDescriptionGroupCollectionProvider apiDescriptionProvider)
    {
        _apiActionMeta = QueryApiActionMeta(apiDescriptionProvider);
    }
        
    private static ApiActionMeta[] QueryApiActionMeta(IApiDescriptionGroupCollectionProvider descriptionProvider)
    {
        var metadata = descriptionProvider.ApiDescriptionGroups.Items.SelectMany(gi => gi.Items)
            .Select(ai => new ApiActionMeta(ai))
            .ToArray();

        AssertActionMethodsUnique(metadata);

        return metadata;
    }

    // Technically, a controller method can have more than one associated Http Method.  
    // Since this feature is not use very often, a check will be made so this is not
    // allowed to simplify the implementation.  NOTE:  it is still valid to have the
    // same URL route associated with two different Http Methods.
    private static void AssertActionMethodsUnique(IEnumerable<ApiActionMeta> metadata)
    {
        var invalidMetadata = metadata.GroupBy(m => m.ActionMethodInfo)
            .Where(g => g.Count() > 1)
            .Select(g => $"Controller: {g.Key.DeclaringType?.FullName ?? ""}  Action: {g.Key.Name}")
            .ToArray();
            
        if (invalidMetadata.Any())
        {
            throw new InvalidOperationException(
                $"The following action methods are not unique by Http Method: {string.Join(',', invalidMetadata)}");
        }
    }
        
    public bool TryGetActionMeta(MethodInfo methodInfo, [MaybeNullWhen(false)]
        out ApiActionMeta actionMeta)
    {
        if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            
        actionMeta = _apiActionMeta.FirstOrDefault(ad => ad.ActionMethodInfo == methodInfo);
        return actionMeta != null;
    }

    public bool TryGetActionMeta(string httpMethod, string relativePath, 
        [MaybeNullWhen(false)] out ApiActionMeta actionMeta)
    {
        if (string.IsNullOrWhiteSpace(httpMethod))
            throw new ArgumentException("Http Method not specified.", nameof(httpMethod));
            
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Relative Path not specified.", nameof(relativePath));

        actionMeta = _apiActionMeta.FirstOrDefault(ad => 
            ad.HttpMethod == httpMethod.ToUpper() && 
            ad.RelativePath == relativePath);

        return actionMeta != null;
    }

    public ApiActionMeta GetActionMeta(MethodInfo methodInfo)
    {
        if (TryGetActionMeta(methodInfo, out ApiActionMeta? actionMeta)) return actionMeta;
            
        throw new InvalidOperationException(
            $"Api Action Metadata not found for method named: {methodInfo.Name} on type: {methodInfo.DeclaringType?.Name}.");
    }

    public ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes) where T : ControllerBase
    {
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("Action Name not specified.", nameof(actionName));
            
        MethodInfo? actionMethod = typeof(T).GetMethod(actionName, paramTypes);
        if (actionMethod == null)
        {
            throw new InvalidOperationException(
                $"Method named {actionName} not found on type: {typeof(T)}");
        }
            
        if (TryGetActionMeta(actionMethod, out ApiActionMeta? actionMeta)) return actionMeta;
             
        throw new InvalidOperationException(
            $"Api Action Metadata not found for method named: {actionName} on type: {typeof(T).Name}.");
    }
}
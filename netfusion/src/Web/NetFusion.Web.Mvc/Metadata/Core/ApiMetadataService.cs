using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Service returning metadata for an MVC applications defined
    /// controllers and routes.
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
            return descriptionProvider.ApiDescriptionGroups.Items.SelectMany(gi => gi.Items)
                .Select(ai => new ApiActionMeta(ai))
                .ToArray();
        }
        
        public bool TryGetActionMeta(MethodInfo methodInfo, out ApiActionMeta actionMeta)
        {
            actionMeta = _apiActionMeta.FirstOrDefault(ad => ad.ActionMethodInfo == methodInfo);
            return actionMeta != null;
        }

        public ApiActionMeta GetActionMeta(MethodInfo methodInfo)
        {
            if (TryGetActionMeta(methodInfo, out ApiActionMeta actionMeta)) return actionMeta;
            
            throw new InvalidOperationException(
                $"Api Action Metadata not found for method named: {methodInfo.Name} on type: {methodInfo.DeclaringType?.Name}.");
        }

        public ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes) where T : ControllerBase
        {
            MethodInfo actionMethod = typeof(T).GetMethod(actionName, paramTypes);
            TryGetActionMeta(actionMethod, out ApiActionMeta actionMeta);
            return actionMeta;
        }
    }
}
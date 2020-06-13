using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Service returning metadata for an MVC applications defined
    /// controllers and routes.
    /// </summary>
    public class ApiMetadataService : IApiMetadataService
    {
        private readonly ApiActionMeta[] _apiActionMeta;

        public ApiMetadataService(
            IApiDescriptionGroupCollectionProvider descriptionProvider)
        {
            _apiActionMeta = QueryApiActionMeta(descriptionProvider);
        }
        
        private static ApiActionMeta[] QueryApiActionMeta(IApiDescriptionGroupCollectionProvider descriptionProvider)
        {
            return descriptionProvider.ApiDescriptionGroups.Items.SelectMany(gi => gi.Items)
                .Select(ai => new ApiActionMeta(ai))
                .ToArray();
        }
        
        public ApiActionMeta GetActionMeta(MethodInfo methodInfo)
        {
            var actionDescriptor = _apiActionMeta.FirstOrDefault(ad => ad.ActionMethodInfo == methodInfo);
            if (actionDescriptor == null)
            {
                
            }

            return actionDescriptor;
        }

        public ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes) where T : ControllerBase
        {
            MethodInfo method = typeof(T).GetMethod(actionName, paramTypes);
            return GetActionMeta(method);
        }
    }
}
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Linq;
using System.Reflection;

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
    }
}
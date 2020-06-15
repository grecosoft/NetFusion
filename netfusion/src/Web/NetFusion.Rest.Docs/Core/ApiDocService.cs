using System.Collections.Concurrent;
using System.Reflection;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core
{
    public class ApiDocService : IApiDocService
    {
        private readonly IApiMetadataService _apiMetadata;
        private readonly ConcurrentDictionary<MethodInfo, ApiActionDoc> _apiMethodDocs;
        
        public ApiDocService(IApiMetadataService apiMetadata)
        {
            _apiMetadata = apiMetadata;
            _apiMethodDocs = new ConcurrentDictionary<MethodInfo, ApiActionDoc>();
        }

        public bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc)
        {
            actionDoc = _apiMethodDocs.GetOrAdd(actionMethodInfo, LoadActionDoc);
            return actionDoc != null;
        }

        private ApiActionDoc LoadActionDoc(MethodInfo actionMethodInfo)
        {
            return _apiMetadata.TryGetActionMeta(actionMethodInfo, out ApiActionMeta actionMeta) 
                ? new ApiActionDoc("Todo", actionMeta) : null;
        }
    }
}
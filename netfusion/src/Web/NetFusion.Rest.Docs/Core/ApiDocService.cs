using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Rest.Docs.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core
{
    public class ApiDocService : IApiDocService
    {
        private readonly ConcurrentDictionary<MethodInfo, ApiActionDoc> _apiMethodDocs;
        
        private readonly IApiMetadataService _apiMetadata;
        private readonly IDocModule _docModule;
        
        public ApiDocService(
            IApiMetadataService apiMetadata,
            IDocModule docModule)
        {
            _apiMethodDocs = new ConcurrentDictionary<MethodInfo, ApiActionDoc>();
            
            _apiMetadata = apiMetadata;
            _docModule = docModule;
        }

        public bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc)
        {
            actionDoc = _apiMethodDocs.GetOrAdd(actionMethodInfo, LoadActionDoc);
            return actionDoc != null;
        }

        private ApiActionDoc LoadActionDoc(MethodInfo actionMethodInfo)
        {
            return _apiMetadata.TryGetActionMeta(actionMethodInfo, out ApiActionMeta actionMeta)
                ? BuildActionDoc(actionMeta)
                : null;
        }

        private ApiActionDoc BuildActionDoc(ApiActionMeta actionMeta)
        {
            var actionDoc = new ApiActionDoc(null, actionMeta.RelativePath, actionMeta.HttpMethod);

            _docModule.ApplyDescriptions<IActionDescription>(desc => desc.Describe(actionDoc, actionMeta));
            
            AssembleParamDocs(actionMeta.RouteParameters, actionDoc.RouteParams);
            AssembleParamDocs(actionMeta.QueryParameters, actionDoc.QueryParams);
            AssembleParamDocs(actionMeta.HeaderParameters, actionDoc.HeaderParams);
        
            AssembleResponseDocs(actionDoc, actionMeta);

            return actionDoc;
        }

        private void AssembleParamDocs(IEnumerable<ApiParameterMeta> paramsMetaItems,
            ICollection<ApiParameterDoc> paramDocs)
        {
            paramsMetaItems.ForEach(paramMeta =>
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.BindingName,
                    IsOptions = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue
                };
                
                _docModule.ApplyDescriptions<IParamDescription>(desc => desc.Describe(paramDoc, paramMeta));
                paramDocs.Add(paramDoc);
            });
        }

        private void AssembleResponseDocs(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            actionMeta.ResponseMeta.ForEach(meta =>
            {
                var responseDoc = new ApiResponseDoc
                {

                };

                _docModule.ApplyDescriptions<IResponseDescription>(desc => desc.Describe(responseDoc, meta));
            });
        }
    }
}
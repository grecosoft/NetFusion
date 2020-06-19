using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Docs.Core.Description;
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
            var context = new Dictionary<string, object>();
            var actionDoc = new ApiActionDoc(actionMeta.RelativePath, actionMeta.HttpMethod);

            _docModule.ApplyDescriptions<IActionDescription>(context, desc => desc.Describe(actionDoc, actionMeta));
            
            AssembleParamDocs(context, actionMeta.RouteParameters, actionDoc.RouteParams);
            AssembleParamDocs(context, actionMeta.QueryParameters, actionDoc.QueryParams);
            AssembleParamDocs(context, actionMeta.HeaderParameters, actionDoc.HeaderParams);
        
            AssembleResponseDocs(context, actionDoc, actionMeta);

            return actionDoc;
        }

        private void AssembleParamDocs(IDictionary<string, object> context,
            IEnumerable<ApiParameterMeta> paramsMetaItems,
            ICollection<ApiParameterDoc> paramDocs)
        {
            paramsMetaItems.ForEach(paramMeta =>
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.BindingName,
                    IsOptions = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue,
                    Type = paramMeta.ParameterType.GetJsTypeName()
                };
                
                _docModule.ApplyDescriptions<IParamDescription>(context, desc => desc.Describe(paramDoc, paramMeta));
                paramDocs.Add(paramDoc);
            });
        }

        private void AssembleResponseDocs(IDictionary<string, object> context,
            ApiActionDoc actionDoc,
            ApiActionMeta actionMeta)
        {
            actionMeta.ResponseMeta.ForEach(meta =>
            {
                var responseDoc = new ApiResponseDoc
                {
                    Statuses = meta.Statuses
                };

                _docModule.ApplyDescriptions<IResponseDescription>(context, desc => desc.Describe(responseDoc, meta));
                actionDoc.ResponseDocs.Add(responseDoc);
            });
        }
    }
}
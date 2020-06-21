using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IApiMetadataService _apiMetadata;
        private readonly IDocDescription[] _docDescriptions;
        
        public ApiDocService(
            IApiMetadataService apiMetadata,
            IDocModule docModule)
        {
            _apiMetadata = apiMetadata;
            _docDescriptions = docModule.GetDocDescriptions().ToArray();
        }

        public bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc)
        {
            actionDoc = null;      
            
            if (_apiMetadata.TryGetActionMeta(actionMethodInfo, out ApiActionMeta actionMeta))
            {
                actionDoc = BuildActionDoc(actionMeta);
                return true;
            }

            return false;
        }

        public bool TryGetActionDoc(string relativePath, out ApiActionDoc actionDoc)
        {
            actionDoc = null;      
            
            if (_apiMetadata.TryGetActionMeta(relativePath, out ApiActionMeta actionMeta))
            {
                actionDoc = BuildActionDoc(actionMeta);
                return true;
            }

            return false;
        }
        
        private ApiActionDoc BuildActionDoc(ApiActionMeta actionMeta)
        {
            var context = new Dictionary<string, object>();
            var actionDoc = new ApiActionDoc(actionMeta.RelativePath, actionMeta.HttpMethod);

            ApplyDescriptions<IActionDescription>(context, desc => desc.Describe(actionDoc, actionMeta));
            
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
                
                ApplyDescriptions<IParamDescription>(context, desc => desc.Describe(paramDoc, paramMeta));
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

                ApplyDescriptions<IResponseDescription>(context, desc => desc.Describe(responseDoc, meta));
                actionDoc.ResponseDocs.Add(responseDoc);
            });
        }
        
        private void ApplyDescriptions<T>(IDictionary<string, object> context, Action<T> description)
            where T : class, IDocDescription
        {
            foreach(T instance in _docDescriptions.OfType<T>())
            {
                instance.Context = context;
                description.Invoke(instance);
            }
        }
    }
}
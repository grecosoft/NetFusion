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
    /// <summary>
    /// Main entry point service responsible for returning documentation
    /// for a given controller's action method. 
    /// </summary>
    public class ApiDocService : IApiDocService
    {
        private readonly IApiMetadataService _apiMetadata;
        private readonly IDocDescription[] _docDescriptions;
        
        public ApiDocService(
            IDocModule docModule,
            IApiMetadataService apiMetadata,
            IEnumerable<IDocDescription> docDescriptions)
        {
            _apiMetadata = apiMetadata ?? throw new ArgumentNullException(nameof(apiMetadata));

            // Instances of the IDocDescription implementations are to be executed
            // based on the order of their types registered within RestDocConfig.
            _docDescriptions = docDescriptions.OrderByMatchingType (
                docModule.RestDocConfig.DescriptionTypes
            ).ToArray();
        }

        public bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc)
        {
            if (actionMethodInfo is null)
            {
                throw new ArgumentNullException(nameof(actionMethodInfo));
            }

            actionDoc = null;      

            // Determines if there is metadata associated with the action method.
            // If metadata is found, builds an action document model.
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

            // Determines if there is metadata associated with the action method.
            // If metadata is found, builds an action document model.
            if (_apiMetadata.TryGetActionMeta(relativePath, out ApiActionMeta actionMeta))
            {
                actionDoc = BuildActionDoc(actionMeta);
                return true;
            }

            return false;
        }

        // Provided the controller action metadata, generates an action document by
        // combining the metadata with additional documentation sources implemented
        // by the derived IDocDescriptions registerations.
        private ApiActionDoc BuildActionDoc(ApiActionMeta actionMeta)
        {
            var context = new DescriptionContext();

            // Create the root document associated with the action metadata.
            var actionDoc = new ApiActionDoc(actionMeta.RelativePath, actionMeta.HttpMethod);

            // Apply all action description registered implementations.
            ApplyDescriptions<IActionDescription>(context, desc => desc.Describe(actionDoc, actionMeta));

            // Apply all parameter description registered implemenations.
            AssembleParamDocs(context, actionMeta.RouteParameters, actionDoc.RouteParams);
            AssembleParamDocs(context, actionMeta.QueryParameters, actionDoc.QueryParams);
            AssembleParamDocs(context, actionMeta.HeaderParameters, actionDoc.HeaderParams);

            // Apply documentation for the possible action responses.
            AssembleResponseDocs(context, actionDoc, actionMeta);

            return actionDoc;
        }

        private void AssembleParamDocs(DescriptionContext context,
            IEnumerable<ApiParameterMeta> paramsMetaItems,
            ICollection<ApiParameterDoc> paramDocs)
        {
            paramsMetaItems.ForEach(paramMeta =>
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.BindingName,
                    IsOptional = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue,
                    Type = paramMeta.ParameterType.GetJsTypeName()
                };
                
                ApplyDescriptions<IParamDescription>(context, desc => desc.Describe(paramDoc, paramMeta));
                paramDocs.Add(paramDoc);
            });
        }

        private void AssembleResponseDocs(DescriptionContext context,
            ApiActionDoc actionDoc,
            ApiActionMeta actionMeta)
        {
            actionMeta.ResponseMeta.ForEach(meta =>
            {
                var responseDoc = new ApiResponseDoc
                {
                    Statuses = meta.Statuses,
                };

                // If there is response resource associated with the response status
                // code, add documentation for the resource type. 
                if (meta.ModelType != null)
                {
                    ApplyDescriptions<IResponseDescription>(context, desc => desc.Describe(responseDoc, meta));
                }
                
                actionDoc.ResponseDocs.Add(responseDoc);
            });
        }
        
        private void ApplyDescriptions<T>(DescriptionContext context, Action<T> description)
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
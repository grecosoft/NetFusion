using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Common.Extensions.Collections;
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
            if (docModule is null)
            {
                throw new ArgumentNullException(nameof(docModule));
            }

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
            // If metadata is found, builds an action document model from the meatadata.
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
            // If metadata is found, builds an action document model from the metadata.
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
            // Create the root document associated with the action metadata.
            var actionDoc = new ApiActionDoc(actionMeta.RelativePath, actionMeta.HttpMethod);

            SetEmbeddedResourceMeta(actionDoc, actionMeta);

            // Apply all action description registered implementations.
            ApplyDescriptions<IActionDescription>(desc => desc.Describe(actionDoc, actionMeta));

            // Apply documentation for the possible action responses.
            AssembleResponseDocs(actionDoc, actionMeta);

            // Describe all embedded resources and relations within the populated action document.
            ApplyDescriptions<IEmbeddedDescription>(desc => desc.Describe(actionDoc, actionMeta));
            ApplyDescriptions<ILinkedDescription>(desc => desc.Describe(actionDoc, actionMeta));

            return actionDoc;
        }

        private static void SetEmbeddedResourceMeta(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var attributes = actionMeta.GetFilterMetadata<EmbeddedResourceAttribute>();
            actionDoc.EmbeddedResourceAttribs = attributes.ToArray();
        }

        private void AssembleResponseDocs(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            foreach(ApiResponseMeta meta in actionMeta.ResponseMeta)
            {
                var responseDoc = new ApiResponseDoc
                {
                    Status = meta.Status,
                };

                // If there is response resource associated with the response status
                // code, add documentation for the resource type. 
                if (meta.ModelType != null)
                {
                    ApplyDescriptions<IResponseDescription>(desc => desc.Describe(responseDoc, meta));
                }

                actionDoc.ResponseDocs.Add(responseDoc);
            }
        }
        
        private void ApplyDescriptions<T>(Action<T> description)
            where T : class, IDocDescription
        {
            foreach(T instance in _docDescriptions.OfType<T>())
            {
                description.Invoke(instance);
            }
        }
    }
}
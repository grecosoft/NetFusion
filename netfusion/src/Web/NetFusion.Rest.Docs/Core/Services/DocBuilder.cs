using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Types;
using NetFusion.Rest.Common;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Resources;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Server.Linking;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Services
{
    /// <summary>
    /// Service responsible for constructing an action documentation model based on its associated metadata.
    /// The model is then augmented with additional documentation such as code comments by invoking associated
    /// IDocDescription classes.
    /// </summary>
    public class DocBuilder : IDocBuilder
    {
        private readonly IResourceMediaModule _resourceMediaModule;
        private readonly IApiMetadataService _apiMetadata;
        private readonly IDocDescription[] _docDescriptions;
        private readonly ITypeCommentService _typeComments;
        
        public DocBuilder(IDocModule docModule,
            IResourceMediaModule resourceMediaModule,
            IApiMetadataService apiMetadata,
            IEnumerable<IDocDescription> docDescriptions,
            ITypeCommentService typeComments)
        {
            if (docModule == null) throw new ArgumentNullException(nameof(docModule));
            if (docDescriptions == null) throw new ArgumentNullException(nameof(docDescriptions));
            
            _resourceMediaModule = resourceMediaModule ?? throw new ArgumentNullException(nameof(resourceMediaModule));
            _apiMetadata = apiMetadata ?? throw new ArgumentNullException(nameof(apiMetadata));
            _typeComments = typeComments ?? throw new ArgumentNullException(nameof(typeComments));
            
            // Instances of the IDocDescription implementations are to be executed
            // based on the order of their types registered within RestDocConfig.
            // These classes are what add the documentation to the APIs metadata.
            _docDescriptions = docDescriptions.OrderByMatchingType (
                docModule.RestDocConfig.DescriptionTypes
            ).ToArray();
        }

        public ApiActionDoc BuildActionDoc(ApiActionMeta actionMeta)
        {
            if (actionMeta == null) throw new ArgumentNullException(nameof(actionMeta));
            
            // Create the root document associated with the action metadata.
            var actionDoc = CreateActionDoc(actionMeta);
            
            SetEmbeddedResourceMeta(actionDoc, actionMeta);
            
            // Add the associated action related documentation.
            AddParameterDocs(actionDoc, actionMeta);
            AddResponseDocs(actionDoc, actionMeta);
            AddEmbeddedDocs(actionDoc);
            AddRelationDocs(actionDoc);

            return actionDoc;
        }

        private ApiActionDoc CreateActionDoc(ApiActionMeta actionMeta)
        {
            var actionDoc = new ApiActionDoc(actionMeta.RelativePath, actionMeta.HttpMethod);
            
            ApplyDescriptions<IActionDescription>(desc => desc.Describe(actionDoc, actionMeta));
            return actionDoc;
        }
        
        private static void SetEmbeddedResourceMeta(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var attributes = actionMeta.GetFilterMetadata<EmbeddedResourceAttribute>();
            actionDoc.EmbeddedResourceAttribs = attributes.ToArray();
        }

        private void AddParameterDocs(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            AddParameterDocs(actionDoc.RouteParams, actionMeta, actionMeta.RouteParameters);
            AddParameterDocs(actionDoc.BodyParams, actionMeta, actionMeta.BodyParameters);
            AddParameterDocs(actionDoc.HeaderParams, actionMeta, actionMeta.HeaderParameters);
            AddParameterDocs(actionDoc.QueryParams, actionMeta, actionMeta.QueryParameters);
        }
        
        private void AddParameterDocs(ICollection<ApiParameterDoc> paramDocs,
            ApiActionMeta actionMeta,
            IEnumerable<ApiParameterMeta> paramsMetaItems)
        {
            foreach (ApiParameterMeta paramMeta in paramsMetaItems)
            {
                var paramDoc = new ApiParameterDoc
                {
                    Name = paramMeta.ParameterName,
                    IsOptional = paramMeta.IsOptional,
                    DefaultValue = paramMeta.DefaultValue?.ToString(),
                    Type = paramMeta.ParameterType.GetJsTypeName()
                };

                if (paramMeta.ParameterType.IsClass && !paramMeta.ParameterType.IsBasicType())
                {
                    paramDoc.ResourceDoc = _typeComments.GetResourceDoc(paramMeta.ParameterType);
                }
                
                ApplyDescriptions<IParameterDescription>(desc => desc.Describe(paramDoc, actionMeta, paramMeta));

                paramDocs.Add(paramDoc);
            }
        }
        
        private void AddResponseDocs(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            foreach(ApiResponseMeta responseMeta in actionMeta.ResponseMeta)
            {
                var responseDoc = new ApiResponseDoc
                {
                    Status = responseMeta.Status,
                };

                // If there is a response resource associated with the response status
                // code, add documentation for the resource type. 
                if (responseMeta.ModelType != null && !responseMeta.ModelType.IsBasicType())
                {
                    responseDoc.ResourceDoc = _typeComments.GetResourceDoc(responseMeta.ModelType);
                }
                
                ApplyDescriptions<IResponseDescription>(desc => desc.Describe(responseDoc, responseMeta));

                actionDoc.ResponseDocs.Add(responseDoc);
            }
        }

        private void AddEmbeddedDocs(ApiActionDoc actionDoc)
        {
            // Process all resource documents across all response documents
            // having an associated resource.
            foreach (ApiResourceDoc resourceDoc in actionDoc.ResponseDocs
                .Where(d => d.ResourceDoc != null)
                .Select(d => d.ResourceDoc))
            {
                ApplyEmbeddedResourceDocs(resourceDoc, actionDoc.EmbeddedResourceAttribs);
            }
        }
        
        private void ApplyEmbeddedResourceDocs(ApiResourceDoc resourceDoc,
            EmbeddedResourceAttribute[] embeddedResources)
        {
            resourceDoc.EmbeddedResourceDocs = GetEmbeddedResourceDocs(resourceDoc, embeddedResources).ToArray();

            // Next recursively process any embedded documents to determine if they
            // have any embedded children resources.
            foreach(ApiResourceDoc embeddedResourceDoc in resourceDoc.EmbeddedResourceDocs
                .Select(er => er.ResourceDoc))
            {
                ApplyEmbeddedResourceDocs(embeddedResourceDoc, embeddedResources);
            }
        }
        
        private IEnumerable<ApiEmbeddedDoc> GetEmbeddedResourceDocs(ApiResourceDoc parentResourceDoc,
            EmbeddedResourceAttribute[] embeddedResources)
        {
            // Find any embedded types specified for the resource type.
            var embeddedAttributes = embeddedResources.Where(et =>
                et.ParentResourceType.GetResourceName() == parentResourceDoc.ResourceName);

            // For each embedded resource type, create an embedded resource document
            // with the documentation for each child embedded resource.
            foreach (EmbeddedResourceAttribute embeddedAttribute in embeddedAttributes)
            {
                var embeddedResourceDoc = new ApiEmbeddedDoc
                {
                    EmbeddedName = embeddedAttribute.EmbeddedName,
                    IsCollection = embeddedAttribute.IsCollection,
                    ResourceDoc = _typeComments.GetResourceDoc(embeddedAttribute.ChildResourceType)
                };

                embeddedResourceDoc.ResourceDoc.ResourceName = embeddedAttribute
                    .ChildResourceType.GetResourceName();
                
                ApplyDescriptions<IEmbeddedDescription>(desc => desc.Describe(embeddedResourceDoc, embeddedAttribute));

                yield return embeddedResourceDoc;
            }
        }

        private void AddRelationDocs(ApiActionDoc actionDoc)
        {
            var (entry, ok) = _resourceMediaModule.GetMediaTypeEntry(InternetMediaTypes.HalJson);
            if (!ok)
            {
                return;
            }

            foreach(ApiResourceDoc resourceDoc in actionDoc.ResponseDocs
                .Where(rd => rd.ResourceDoc != null)
                .Select(rd => rd.ResourceDoc))
            {
                AddRelationDoc(resourceDoc, entry);
            }
        }
        
        private void AddRelationDoc(ApiResourceDoc resourceDoc, MediaTypeEntry mediaTypeEntry)
        {
            var (meta, ok) = mediaTypeEntry.GetResourceTypeMeta(resourceDoc.ResourceType);
            if (ok)
            {
                foreach(ResourceLink resourceLink in meta.Links)
                {
                    var relationDoc = new ApiRelationDoc
                    {
                        Name = resourceLink.RelationName,
                        Method = resourceLink.Method,
                    };

                    SetLinkDetails(relationDoc, (dynamic)resourceLink);

                    ApplyDescriptions<IRelationDescription>(desc => desc.Describe(resourceDoc, relationDoc, resourceLink));
                    resourceDoc.RelationDocs.Add(relationDoc);
                }
            }

            // Next check any embedded resources:
            foreach (ApiResourceDoc embeddedResourceDoc in resourceDoc.EmbeddedResourceDocs
                .Select(er => er.ResourceDoc))
            {
                AddRelationDoc(embeddedResourceDoc, mediaTypeEntry);
            }
        }

        private static void SetLinkDetails(ApiRelationDoc relationDoc, ResourceLink resourceLink)
        {
            relationDoc.HRef = resourceLink.Href;
        }
        
        private void SetLinkDetails(ApiRelationDoc relationDoc, ControllerActionLink resourceLink)
        {
            relationDoc.HRef = _apiMetadata.GetActionMeta(resourceLink.ActionMethodInfo).RelativePath;
        }
        
        private void SetLinkDetails(ApiRelationDoc relationDoc, TemplateUrlLink resourceLink)
        {
            relationDoc.HRef = _apiMetadata.GetActionMeta(resourceLink.ActionMethodInfo).RelativePath;
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
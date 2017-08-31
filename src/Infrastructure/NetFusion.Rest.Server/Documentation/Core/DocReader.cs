using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Server.Actions;
using NetFusion.Rest.Server.Documentation.Modules;
using NetFusion.Rest.Server.Generation;
using NetFusion.Rest.Server.Meta;
using NetFusion.Rest.Server.Modules;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Responsible for aggregating documentation for a controller's action method.
    /// </summary>
    public class DocReader : IDocReader
    {
        private readonly IDocModule _docModule;
        private readonly IResourceMediaModule _resourceMediaModule;
        private readonly IResourceTypeReader _typeReader;

		public DocReader(
            IDocModule docModule,
            IResourceMediaModule resourceMediaModule,
            IResourceTypeReader typeReader)
        {
            _docModule = docModule;
            _resourceMediaModule = resourceMediaModule;
            _typeReader = typeReader;
        }

        public async Task<DocActionModel> GetActionDocModel(IHeaderDictionary headers, MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo), "Method information not specified.");

            DocAction actionDoc = _docModule.GetActionDoc(methodInfo);
            if (actionDoc == null)
            {
               return await Task.FromResult<DocActionModel>(null);
            }

            var actionDocModel = new DocActionModel(actionDoc);
            await AddResourceDocs(actionDocModel, actionDoc);

            Type responseResourceType = ActionExtensions.GetActionResponseResourceType(methodInfo);
            if (responseResourceType != null)
            {
                // Based on the accept type, load the resource metadata which will contain information
                // about the associated link relations.  Add documentation for each resource link.
                IResourceMeta resourceMeta = _resourceMediaModule.GetRequestedResourceMediaMeta(headers, responseResourceType);

                AddResourceRelationDocs(actionDocModel, actionDoc, resourceMeta);
            }

            return actionDocModel;
        }

        private void AddResourceRelationDocs(DocActionModel actionDocModel, DocAction actionDoc, IResourceMeta resourceMeta)
        {
            DocRelation[] relationDocs = resourceMeta.Links.Select(
                link => new DocRelation(link)
                {
                    Description = GetRelationDescription(link, actionDoc, _docModule.GetCommonDefinitions())
                }).ToArray();

            actionDocModel.Relations = relationDocs;
        }

        private string GetRelationDescription(ActionLink actionLink, DocAction actionDoc, CommonDefinitions commmonDefs)
        {
            // Determines the documentation for a given relation by first checking if there was a specific 
            // description specified for the relation for the given action method.  If not, the common relation
            // description is used.
            return
                actionDoc.Relations.FirstOrDefault(r => r.RelName == actionLink.RelationName)?.Description
                ?? commmonDefs.Relations.FirstOrDefault(r => r.RelName == actionLink.RelationName)?.Description;
        }

        public async Task AddResourceDocs(DocActionModel actionDocModel, DocAction actionDoc)
        {
            if (actionDocModel == null)
                throw new ArgumentNullException(nameof(actionDocModel), "Action Document Model not specified.");

            if (actionDoc == null)
                throw new ArgumentNullException(nameof(actionDoc), "Action Document not specified.");

            if (actionDoc.RequestResourceType != null)
            {
                actionDocModel.ReqestResource = _docModule.GetResourceDoc(actionDoc.RequestResourceType);
                actionDocModel.RequestTypeScript = await ReadTypeScriptDefinition(actionDoc.RequestResourceType);
            }

            if (actionDoc.ResponseResourceType != null)
            {
                actionDocModel.ResponseResource = _docModule.GetResourceDoc(actionDoc.ResponseResourceType);
                actionDocModel.ResponseTypeScript = await ReadTypeScriptDefinition(actionDoc.ResponseResourceType);
            }
        }

        private async Task<string> ReadTypeScriptDefinition(Type resourceType)
        {
            using (var memoryStsream = await _typeReader.ReadTypeDefinitonFile(resourceType))
            using (var streamReader = new StreamReader(memoryStsream))
            {
                return await streamReader.ReadToEndAsync();
            }
        } 
    }
}

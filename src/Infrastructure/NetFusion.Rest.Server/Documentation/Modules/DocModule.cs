using Autofac;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources.Doc;
using NetFusion.Rest.Server.Actions;
using NetFusion.Rest.Server.Documentation.Core;
using NetFusion.Rest.Server.Modules;
using NetFusion.Rest.Server.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace NetFusion.Rest.Server.Documentation.Modules
{
    /// <summary>
    /// Plug-in module responsible for lazy loading the controller action method documentation.  
    /// This code is not executed until the a request is made for documentation.  The documentation 
    /// associated for each action method is lazy loaded upon the first request.
    /// </summary>
    public class DocModule : PluginModule, 
        IDocModule
    {
        private Dictionary<Type, ActionRegistration[]> _actionDocs; // Controller --> Action Doc
        private Dictionary<Type, Lazy<DocResource>> _resourceDocs;  // Resource --> Resource Doc
        private EnvironmentSettings _environmentSettings;
        private CommonDefinitions _commonDefinitions;

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register component used to read action related documentation items
            // such as resource documentation and corresponding type-script definitions.
            builder.RegisterType<DocReader>()
                   .As<IDocReader>()
                   .InstancePerLifetimeScope();
        }

        public DocAction GetActionDoc(MethodInfo actionMethod)
        {
            if (actionMethod == null) throw new ArgumentNullException(nameof(actionMethod), 
                "Action Method cannot be null.");

            Type controllerType = actionMethod.DeclaringType;

            // Locate the controller documentation then lazy load the documentation
            // for the specific action method.
            if (_actionDocs.TryGetValue(controllerType, out ActionRegistration[] actionRegs))
            {
                var actionReg = actionRegs.FirstOrDefault(r => r.ActionMethod == actionMethod);
                return actionReg?.ActionDocument.Value; // Lazy Load
            }
            return null;
        }

        // Contains descriptions for documentation items that can be shared across actions.
        public CommonDefinitions GetCommonDefinitions() => _commonDefinitions;

        public DocResource GetResourceDoc(Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType), 
                "Resource Type cannot be null.");

            if(_resourceDocs.TryGetValue(resourceType, out Lazy<DocResource> resourceDoc))
            {
                return resourceDoc.Value; // Lazy Load
            }
            return null;
        }

        public override void StartModule(IContainer container, ILifetimeScope scope)
        {
            _environmentSettings = scope.Resolve<EnvironmentSettings>();

            LoadCommonDefinitions();

            LazyLoadControllerDocs();
            LazyLoadResourceDocs();
        }

        //------------- COMMON DEFAULT API DOCUMENTATION -----------------

        private string CommonDefinitionPath =>
            Path.Combine(_environmentSettings.GetControllerDocPath(),
            "CommonDefinitions.xml");

        // Loads default descriptions that can be globally applied if not
        // overridden by action specific descriptions.
        private void LoadCommonDefinitions()
		{
            XElement xElement = null;
			if (!File.Exists(CommonDefinitionPath))
			{
				_commonDefinitions = new CommonDefinitions();
			}
            else
            {
                xElement = XElement.Load(CommonDefinitionPath);
                if (xElement == null)
                {
                    _commonDefinitions = new CommonDefinitions();
                }
            }

            if (xElement == null)
            {
                return;
            }
			
			_commonDefinitions = new CommonDefinitions
			{
				HttpCodes = GetActionDocHttpCodes(xElement).ToArray(),
				Relations = GetDocRelations(xElement).ToArray(),
				EmbeddedResources = GetEmbeddedResources(xElement).ToArray()
			};
		}

		private IEnumerable<DocHttpCode> GetActionDocHttpCodes(XElement actionDocElem)
		{
            return actionDocElem.Elements("http-codes")
                .Elements("code")
                .Select(e => new DocHttpCode(e));
		}

        private IEnumerable<DocRelation> GetDocRelations(XElement actionDocElem)
        {
			return actionDocElem.Elements("relations")
				.Elements("relation")
				.Select(e => new DocRelation(e));
        }

		private IEnumerable<DocEmbeddedResource> GetEmbeddedResources(XElement actionDocElem)
		{
			return actionDocElem.Element("embedded-resources")
				.Elements("embedded")
				.Select(e => new DocEmbeddedResource(e));
		}

        // -------------- LAZY LOADED DOCUMENTATION -------------

        // Populates dictionary where the key is the controller type and the
        // value is an array of action documentation to can be lazy loaded.
        private void LazyLoadControllerDocs()
        {
            _actionDocs = new Dictionary<Type, ActionRegistration[]>();

            var controllerTypes = Context.AllPluginTypes
				.Where(pt => pt.IsDerivedFrom<Controller>())
				.ToArray();

            AssertUniqueActionIds(controllerTypes);

            foreach (Type controllerType in controllerTypes)
            {
                var actionDocs = LazyLoadActionDoc(controllerType);
                _actionDocs[controllerType] = actionDocs.ToArray();
            }
        }

        // Returns a list of action documentation for a given controller to be
        // lazy loaded upon the first request.
        private IEnumerable<ActionRegistration> LazyLoadActionDoc(Type controllerType)
        {
            MethodInfo[] actionMethods = controllerType.GetActionMethods().ToArray();

            foreach (MethodInfo actionMethod in actionMethods)
            {
                var lazyActionDoc = new Lazy<DocAction>(() => {
                    var actionDoc = new DocAction(actionMethod)
                    {
                        RequestResourceType = ActionExtensions.GetActionRequestResourceType(actionMethod),
                        ResponseResourceType = ActionExtensions.GetActionResponseResourceType(actionMethod)
                    };

                    SetExternalDoc(actionDoc);
                    return actionDoc;
                });

                yield return new ActionRegistration(actionMethod, lazyActionDoc);
            }
        }

        private void AssertUniqueActionIds(Type[] controllerTypes)
        {
            var allActionMethods = controllerTypes.SelectMany(ct => ct.GetActionMethods());

            var actionIds = allActionMethods
                .Select(m => m.GetAttribute<DocActionAttribute>()?.Id)
                .Where(id => id != null);

            var duplicateIds = actionIds.WhereDuplicated(v => v).ToArray();
            if (duplicateIds.Any())
            {
                throw new InvalidCastException(
                    $"Controller ActionId must be unique.  Duplicates: {string.Join(" | ", duplicateIds)}");
            }
        }

        // -------------- ACTION DOCUMENTATION ---------------

        private string ControllerDocPath(DocAction actionDoc) 
        {
			string controllerName = actionDoc.ActionMethodInfo.DeclaringType.Name;
			return Path.Combine(_environmentSettings.GetControllerDocPath(), controllerName + ".xml");
		}
            
        // Loads the action documentation stored within an external XML document
        // containing documentation for a given controller.
        private void SetExternalDoc(DocAction actionDoc)
        {
            string controllerDocPath = ControllerDocPath(actionDoc);
            if (!File.Exists(controllerDocPath))
            {
                return;
            }

            var xElement = XElement.Load(controllerDocPath);
            var actionDocElem = xElement.Elements("actions").Elements("action")
                .FirstOrDefault(e => e.Attribute("doc-id").Value == actionDoc.ActionId);

            if (actionDocElem != null)
            {
                actionDoc.Description = actionDocElem.Elements("action-doc").FirstOrDefault()?.Value;
                actionDoc.ResponseDescription = actionDocElem.Elements("response-doc").FirstOrDefault()?.Value;

                SetActionParams(actionDoc, actionDocElem);
                SetHttpStatusCodes(actionDoc, actionDocElem);
                SetEmbeddedResources(actionDoc, actionDocElem);

                actionDoc.Relations = GetDocRelations(actionDocElem).ToArray();
            }
        }

        private void SetActionParams(DocAction actionDoc, XElement actionDocElem)
        {
            // The actual list of arguments received by the action method.
            var actionParams = actionDoc.ActionMethodInfo.GetParameters()
                .Select(p => new
                {
                    p.Name,
                    p.ParameterType
                });

            // Developer configured argument documentation stored in XML.
            var xmlParamDocs = actionDocElem.Elements("route-params")
                .Elements("param")
                .Select(e => new DocActionParam(e))
                .ToArray();

            var actionParamDocs = new List<DocActionParam>();
            foreach(var actionParam in actionParams)
            {
                var xmlPramDoc = xmlParamDocs.FirstOrDefault(p => p.Name == actionParam.Name);
                var paramDoc = new DocActionParam(actionParam.Name, actionParam.ParameterType);

                if (xmlPramDoc != null)
                {
                    paramDoc.Description = xmlPramDoc.Description;
                }

                actionParamDocs.Add(paramDoc);
            }

            actionDoc.ActionParams = actionParamDocs.ToArray();
        }

        private void SetHttpStatusCodes(DocAction actionDoc, XElement actionDocElem)
        {
            IEnumerable<DocHttpCode> xmlDocHttpCodes = GetActionDocHttpCodes(actionDocElem);

            // Determine documentation for each HTTP status code specified on the action
            // method specified by the DocAction Attribute.
            foreach (DocHttpCode httpCode in actionDoc.HttpCodes)
            {
                // Check if specific description is specified and use it.  If not check if a
                // common description is specified.
                DocHttpCode docHttpCode = xmlDocHttpCodes.FirstOrDefault(c => c.Code == httpCode.Code);
                if (docHttpCode == null)
                {
                    docHttpCode = _commonDefinitions.HttpCodes.FirstOrDefault(c => c.Code == httpCode.Code);
                }

                httpCode.Description = docHttpCode?.Description;
            }
        }

        public void SetEmbeddedResources(DocAction docAction, XElement actionDocElem)
        {
            IEnumerable<DocEmbeddedResource> xmlDocEmbedded = GetEmbeddedResources(actionDocElem);
            var embededResDocs = new List<DocEmbeddedResource>();

            // For each embedded resource name specified by the DocEmbeddedResource Attribute on the action
            // method lookup the resource's documentation.
            foreach (ActionEmbeddedName embeddedName in docAction.EmbeddedNames)
            {
                // Check if the controller specific documentation contains description for embedded resource.
                // If not found, use the common description, otherwise use the resource description specified in code.
                DocEmbeddedResource resourceDoc = xmlDocEmbedded.FirstOrDefault(r => r.EmbeddedName == embeddedName.Name) 
                      ?? _commonDefinitions.EmbeddedResources.FirstOrDefault(r => r.EmbeddedName == embeddedName.Name);
                 
                if (resourceDoc == null)
                {
                    string resourceDesc = embeddedName.ResourceType.GetResourceDescription();
                    resourceDoc = new DocEmbeddedResource(embeddedName.Name, resourceDesc);
                } 
                
                if (resourceDoc != null)
                {
                    embededResDocs.Add(resourceDoc);
                }
            }

            docAction.EmbeddedResources = embededResDocs.ToArray();
        }

        // Lazy loads resource documentation form attributes specified on the resource classes.
        private void LazyLoadResourceDocs()
        {
            DocResource LoadResourceDoc(Type resourceType)
            {
                var resourceDoc = resourceType.GetAttribute<DocResourceAttribute>();
                return new DocResource(resourceType, resourceDoc);
            }

            _resourceDocs = Context.AllPluginTypes
                .Where(pt =>
                    pt.IsReturnableResourceType() &&
                    pt.HasAttribute<DocResourceAttribute>())

                .ToDictionary(
                    rt => rt, 
                    rt => new Lazy<DocResource>(() => LoadResourceDoc(rt)));
        }
    }
}

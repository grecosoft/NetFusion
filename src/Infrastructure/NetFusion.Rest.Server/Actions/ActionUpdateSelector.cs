using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Contains methods for searching a controller's action methods used to
    /// create, updated, and delete a resource based on a set of conventions.
    /// </summary>
    public class ActionUpdateSelector
    {
        private readonly Type _controllerType;
        private readonly Type _resourceType;

        public ActionUpdateSelector(
            Type controllerType,
            Type resourceType)
        {
            _controllerType = controllerType;
            _resourceType = resourceType;
        }

        public ActionUrlLink FindCreateResourceAction()
        {
            return GetResourceCreateActions().ToActionLink()
                .FirstOrDefault();
        }

        public ActionUrlLink FindUpdateResourceAction()
        {
            return GetResourceUpdateActions().ToActionLink()
                .FirstOrDefault();
        }

        public ActionUrlLink FindDeleteResourceAction()
        {
            return GetResourceDeleteActions().ToActionLink()
                .FirstOrDefault();
        }

        private IEnumerable<ActionMethodInfo> GetResourceCreateActions()
        {
            return _controllerType.GetActionMethods()
                 .FindActionMethods(_resourceType,
                     acceptsIdentityArg: false,
                     acceptsResourceArg: true,
                     httpMethods: new[] { HttpMethod.Post } )
                 .Where(m => m.Methods.Count() == 1);
        }

        private IEnumerable<ActionMethodInfo> GetResourceUpdateActions()
        {
            return _controllerType.GetActionMethods()
                 .FindActionMethods(_resourceType,
                     acceptsIdentityArg: true,
                     acceptsResourceArg: true,
                      httpMethods: new[] { HttpMethod.Put, HttpMethod.Post })
                 .Where(m => m.Methods.Count() == 1);
        }

        // NOTE:  Since the delete convention is that there is a Delete action method having
        // an argument identified as the resource identity, this could match multiple resource
        // types.  Since the resource type is not specified for a delete, all Delete action 
        // Methods must specify the ResourceType Attribute to specify the resource type it
        // corresponds.
        private IEnumerable<ActionMethodInfo> GetResourceDeleteActions()
        {
            return _controllerType.GetActionMethods()
                 .FindActionMethods(_resourceType,
                     acceptsIdentityArg: true,
                     acceptsResourceArg: false,
                     returnsResourceType: true, 
                     httpMethods: new[] { HttpMethod.Delete })
                 .Where(m => m.Methods.Count() == 1);
        }
    }
}

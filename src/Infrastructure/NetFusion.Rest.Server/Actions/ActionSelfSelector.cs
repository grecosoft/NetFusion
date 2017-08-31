using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// Selects the controller action method based on conventions that should
    /// be invoked to query the resource.  This corresponds to the HTTP Self
    /// relation type.
    /// </summary>
    public class ActionSelfSelector 
    {
        private readonly Type _controllerType;
        private readonly Type _resourceType;

        public ActionSelfSelector(
            Type controllerType,
            Type resourceType)
        {
            _controllerType = controllerType;
            _resourceType = resourceType;
        }

        public ActionUrlLink FindSelfResourceAction()
        {
            return GetResourceSelfActions()
                .ToActionLink()
                .FirstOrDefault();
        }

        private IEnumerable<ActionMethodInfo> GetResourceSelfActions()
        {
            return _controllerType.GetActionMethods()
                .FindActionMethods(_resourceType, 
                    acceptsIdentityArg: true, 
                    returnsResourceType: true, 
                    httpMethods: new[] { HttpMethod.Get })
                .ToArray();
        }
    }
}

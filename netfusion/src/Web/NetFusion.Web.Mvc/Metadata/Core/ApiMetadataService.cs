using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Service returning metadata for an MVC applications defined
    /// controllers and routes.
    /// </summary>
    public class ApiMetadataService : IApiMetadataService
    {
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
        private readonly Lazy<ApiGroupMeta[]> _metadata;

        public ApiMetadataService(
            IApiDescriptionGroupCollectionProvider descriptionProvider)
        {
            _descriptionProvider = descriptionProvider;
            _metadata = new Lazy<ApiGroupMeta[]>(LoadMetadata);
        }

        public ApiGroupMeta[] GetApiGroups() => _metadata.Value;

        public ApiGroupMeta[] GetApiGroup(string groupName)
        {
            if (groupName == null) throw new ArgumentNullException(nameof(groupName));
            
            return GetApiGroups().Where(g => g.GroupName == groupName)
                .ToArray();
        }

        public ApiActionMeta GetApiAction(string groupName, string actionName)
        {
            if (actionName == null) throw new ArgumentNullException(nameof(actionName));
            
            ApiGroupMeta apiGroup = GetApiGroup(groupName).FirstOrDefault();
            if (apiGroup == null)
            {
                throw new InvalidOperationException(
                    $"The controller group name of: {groupName} was not configured.");
            }

            ApiActionMeta apiAction = apiGroup.Actions.FirstOrDefault(
               a => a.ActionName == actionName);
               
            if (apiAction == null)
            {
                throw new InvalidOperationException(
                    $"The controller action named: {actionName} for controller group: {groupName} is not configured.");
            }
            return apiAction;
        }

        private ApiGroupMeta[] LoadMetadata()
        {
            return PopulateGroupMeta().ToArray();
        }

        private IEnumerable<ApiGroupMeta> PopulateGroupMeta()
        {
            var groups = _descriptionProvider.ApiDescriptionGroups.Items
                .Where(d => d.GroupName != null);
            
            foreach (ApiDescriptionGroup group in groups)
            {
                var actions = GetGroupActions(group.Items);
                yield return new ApiGroupMeta(group.GroupName, actions.ToArray());
            }
        }

        private static IEnumerable<ApiActionMeta> GetGroupActions(IEnumerable<ApiDescription> descriptions)
        {
            foreach(ApiDescription description in descriptions)
            {
                if (description.ActionDescriptor is ControllerActionDescriptor actionDescriptor 
                    && IsDiscoverableAction(actionDescriptor))
                {
                    yield return new ApiActionMeta(description, actionDescriptor);
                }
            }
        }

        private static bool IsDiscoverableAction(ControllerActionDescriptor actionDescriptor)
        {
            return actionDescriptor.MethodInfo.HasAttribute<ActionMetaAttribute>();
        }
    }
}
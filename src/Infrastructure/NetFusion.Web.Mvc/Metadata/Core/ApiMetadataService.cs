using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Common;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Service returning metadata for an MVC applications defined
    /// controllers and routes.
    /// </summary>
    public class ApiMetadataService : IApiMetadataService
    {
        private readonly IApiDescriptionGroupCollectionProvider _descriptionProvider;
        private Lazy<ApiGroupMeta[]> _metadata;

        public ApiMetadataService(
            IApiDescriptionGroupCollectionProvider descriptionProvider)
        {
            _descriptionProvider = descriptionProvider;
            _metadata = new Lazy<ApiGroupMeta[]>(LoadMetadata);
        }

        public ApiGroupMeta[] GetApiGroups() => _metadata.Value;

        public ApiGroupMeta[] GetApiGroup(string groupName)
        {
            Check.NotNull(groupName, nameof(groupName));

            return _metadata.Value.Where(g => g.GroupName == groupName)
                .ToArray();
        }

        public ApiActionMeta GetApiAction(string groupName, string actionName)
        {
            Check.NotNull(actionName, nameof(actionName));

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
            var groups = _descriptionProvider.ApiDescriptionGroups.Items;
            foreach (ApiDescriptionGroup group in groups)
            {
                var actions = GetGroupActions(group.Items);
                yield return new ApiGroupMeta(group.GroupName, actions.ToArray());
            }
        }

        private IEnumerable<ApiActionMeta> GetGroupActions(IReadOnlyList<ApiDescription> descriptions)
        {
            foreach(ApiDescription description in descriptions)
            {
                var actionDescriptor = description.ActionDescriptor as ControllerActionDescriptor;
                if (actionDescriptor != null && IsDiscoverableAction(actionDescriptor))
                {
                    yield return new ApiActionMeta(description, actionDescriptor);
                } 
            }
        }

        private bool IsDiscoverableAction(ControllerActionDescriptor actionDescriptor)
        {
            return actionDescriptor.MethodInfo.HasAttribute<ActionMetaAttribute>();
        }
    }
}
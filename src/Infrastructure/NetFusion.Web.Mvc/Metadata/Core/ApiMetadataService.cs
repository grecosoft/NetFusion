using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using NetFusion.Common.Extensions.Reflection;

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

        public ApiGroupMeta[] GetApiGroup(string groupName) => _metadata.Value
            .Where(g => g.GroupName == groupName)
            .ToArray();

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
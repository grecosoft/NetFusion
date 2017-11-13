using NetFusion.Common.Extensions.Types;
using NetFusion.Web.Mvc.Metadata.Models;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Maps the internal classes containing the group and action metadata into 
    /// models returned from the service API.
    /// </summary>
    public static class MetadataMap
    {
        public static ApiServiceInfo GetModel(ApiGroupMeta[] groups)
        {
            return new ApiServiceInfo
            {
                Groups = groups.ToDictionary(g => g.GroupName,
                    g => new GroupInfo {
                        Name = g.GroupName,
                        Actions = GetActions(g)
                    })
            };
        }

        private static IDictionary<string, ActionInfo> GetActions(ApiGroupMeta group)
        {
            return group.Actions.ToDictionary(a => a.ActionName, 
                a => new ActionInfo {
                    Name = a.ActionName,
                    Method = a.HttpMethod,
                    Path = a.RelativePath,
                    Parameters = GetParameters(a).ToArray()
                });
        }

        private static IEnumerable<ParameterInfo> GetParameters(ApiActionMeta action)
        {
            return action.Parameters
                .Select(p => new ParameterInfo {
                    Name = p.ParameterName,
                    IsOptional = p.IsOptional,
                    Type = p.ParameterType.GetJsTypeName(),
                    DefaultValue = p.DefaultValue
                });
        }
    }
}

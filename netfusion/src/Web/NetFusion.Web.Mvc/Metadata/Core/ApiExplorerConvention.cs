using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Convention determining if a given controller's routes should be discoverable
    /// by the ApiExplorer.
    /// </summary>
    public class ApiExplorerConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (ControllerModel model in application.Controllers)
            {
                model.ApiExplorer.IsVisible = false;

                var discoverableAttrib = model.ControllerType.GetAttribute<GroupMetaAttribute>();
                if (discoverableAttrib != null)
                {
                    model.ApiExplorer.IsVisible = true; // Adds controller actions to ApiExplorer.
                    model.ApiExplorer.GroupName = discoverableAttrib.GroupName ?? model.ControllerName;
                }
            }
        }
    }
}
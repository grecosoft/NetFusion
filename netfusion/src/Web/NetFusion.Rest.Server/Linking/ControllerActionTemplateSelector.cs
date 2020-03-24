using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions.Expressions;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Web.Mvc.Metadata;
// ReSharper disable SuggestBaseTypeForParameter

// ReSharper disable UnusedTypeParameter
namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// Selects a controller's action method used to find the corresponding route
    /// template for a resource related link.  Controller action methods having up to
    /// 5 parameters can be used.
    /// </summary>
    /// <typeparam name="TController">The controller to select action methods from
    /// corresponding to template URLs.</typeparam>
    public class ControllerActionTemplateSelector<TController>
        where TController : ControllerBase
    {
        private readonly TemplateUrlLink _link;
        private readonly LambdaExpression _action;

        public ControllerActionTemplateSelector(TemplateUrlLink link, LambdaExpression action)
        {
            _action = action;
            _link = link;
        }

        // Gets the method-info for the expression identifying a controller's action method.
        // The metadata attributes for the corresponding action and its defining controller
        // and found.  The names associated with these two attributes are used to determine
        // the value of the URL template when returning the resource by the formatter.
        public void SetTemplateInfo()
        {          
            MethodInfo action = _action.GetCallMethodInfo();
            _link.ActionTemplateName = GetActionMetaName(action);
            _link.GroupTemplateName = GetControllerMetaName(action.DeclaringType);
        }

        private string GetActionMetaName(MethodInfo action)
        {
            var attrib = action.GetAttribute<ActionMetaAttribute>();
            if (attrib == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ActionMetaAttribute)} not specified on action: {action.Name} for controller: {action.DeclaringType?.FullName}.");
            }
            return attrib.ActionName;
        }

        private string GetControllerMetaName(Type controllerType)
        {
            var attrib = controllerType.GetAttribute<GroupMetaAttribute>();
            if (attrib == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GroupMetaAttribute)} not specified on controller: {controllerType.FullName}.");
            }
            return attrib.GroupName;                          
        }
    }
}

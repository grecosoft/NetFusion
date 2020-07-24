using System;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Rest.Resources
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Returns the resource name associated with a model type.  The resource name
        /// of the model is exposed from the RestApi and becomes part of the contract.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>The resource name specified by ResourceAttribute.
        /// If the attribute is not present, the class name of the model is returned.</returns>
        public static string GetResourceName(this Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType),
                "Model Type cannot be null.");

            return modelType.GetAttribute<ResourceAttribute>()?.ResourceName ?? modelType.Name;
        }
    }
}

using System;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Rest.Resources
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Returns the embedded name associated with a given model type
        /// used to identity embedded resources and models.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>The embedded name specified by the NamedResource attribute.
        /// If the attribute is not present, null is returned.</returns>
        public static string GetExposedResourceTypeName(this Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType),
                "Model Type cannot be null.");

            return modelType.GetAttribute<ExposedNameAttribute>()?.ResourceName;
        }
    }
}

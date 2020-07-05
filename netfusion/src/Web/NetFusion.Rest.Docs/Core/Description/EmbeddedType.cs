using System;

namespace NetFusion.Rest.Docs.Core.Description
{
    /// <summary>
    /// Additional metadata populated from controler action attributes
    /// used to specify the type of embedded resources returned.
    /// </summary>
    public class EmbeddedType
    {
        /// <summary>
        /// The parent resource type containing a child embedded resource.
        /// </summary>
        public Type ParentResourceType { get; }

        /// <summary>
        /// The type of the child embedded within the parent.
        /// </summary>
        public Type ChildResourceType { get; }

        /// <summary>
        /// Name describing the meaning of the embedded resource.
        /// </summary>
        public string EmbeddedName { get; }

        public EmbeddedType(Type parentModelType, Type childModelType, string embeddedName)
        {
            ParentResourceType = parentModelType ?? throw new ArgumentNullException(nameof(parentModelType));
            ChildResourceType = childModelType ?? throw new ArgumentNullException(nameof(childModelType));
            EmbeddedName = embeddedName ?? throw new ArgumentNullException(nameof(embeddedName));
        }
    }
}

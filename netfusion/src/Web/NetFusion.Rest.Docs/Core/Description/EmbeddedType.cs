using System;

namespace NetFusion.Rest.Docs.Core.Description
{
    public class EmbeddedType
    {
        public Type ParentModelType { get; }
        public Type ChildModelType { get; }

        public string EmbeddedName { get; }

        public EmbeddedType(Type parentModelType, Type childModelType, string embeddedName)
        {
            ParentModelType = parentModelType;
            ChildModelType = childModelType;
            EmbeddedName = embeddedName;
        }
    }
}

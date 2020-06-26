using System;
using System.Linq;
using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Core.Description
{
    public class EmbeddedType
    {
        public Type ModelType { get; }
        public string[] Names { get; }

        public EmbeddedType(Type modelType, IEnumerable<string> names)
        {
            ModelType = modelType;
            Names = names.ToArray();
        }
    }
}

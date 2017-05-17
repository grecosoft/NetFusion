using System;

namespace NetFusion.Common.Extensions.Reflection
{

    public class GenericTypeInfo
    {
        public Type Type { get; set; }
        public Type[] GenericArguments { get; set; }
    }
}

using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Core.Description
{
    public class DescriptionContext
    {
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        public ITypeCommentService TypeComments { get; }

        public DescriptionContext(ITypeCommentService typeComments)
        {
            TypeComments = typeComments;
        }
    }
}
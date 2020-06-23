using System;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface ITypeCommentService
    {
        ApiResourceDoc GetResourceDoc(Type resourceType);
    }
}
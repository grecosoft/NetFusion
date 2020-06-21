using System.Reflection;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs
{
    public interface IApiDocService
    {
        bool TryGetActionDoc(MethodInfo actionMethodInfo, out ApiActionDoc actionDoc);
        bool TryGetActionDoc(string relativePath, out ApiActionDoc actionDoc);
    }
}
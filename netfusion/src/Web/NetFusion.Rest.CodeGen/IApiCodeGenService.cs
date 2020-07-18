using System.IO;

namespace NetFusion.Rest.CodeGen
{
    public interface IApiCodeGenService
	{
		bool TryGetResourceCodeFile(string resourceName, out Stream stream);
	}
}

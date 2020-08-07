using System.IO;

namespace NetFusion.Rest.CodeGen
{
	/// <summary>
	/// Service providing access to generated TypeScript corresponding
	/// to the C# models exposed by the API.
	/// </summary>
    public interface IApiCodeGenService
	{
		/// <summary>
		/// Looks up the generated code corresponding to a resource. 
		/// </summary>
		/// <param name="resourceName">The name of the resource.</param>
		/// <param name="stream">If the resource's corresponding code file is
		/// found, a reference to the corresponding stream will be returned.
		/// </param>
		/// <returns>True if the code-generated Typescript with the specified
		/// resource name is found.  Otherwise, False is returned.</returns>
		bool TryGetResourceCodeFile(string resourceName, out Stream stream);
	}
}

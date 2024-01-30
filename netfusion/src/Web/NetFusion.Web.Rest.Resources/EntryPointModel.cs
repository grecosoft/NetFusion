 namespace NetFusion.Web.Rest.Resources; 

 /// <summary>
 /// Model containing information about the Api.
 /// </summary>
 public class EntryPointModel(string version, string apiDocUrl)
 {
     /// <summary>
     /// Value indicating the version of the API.
     /// </summary>
     public string Version { get; } = version;

     /// <summary>
     /// Optional URL to document describing the API. 
     /// </summary>
     public string ApiDocUrl { get; } = apiDocUrl;
 }
 namespace NetFusion.Web.Rest.Resources; 

 /// <summary>
 /// Model containing information about the Api.
 /// </summary>
 public class EntryPointModel
 {
     /// <summary>
     /// Value indicating the version of the API.
     /// </summary>
     public string Version { get; }
        
     /// <summary>
     /// Optional URL to document describing the API. 
     /// </summary>
     public string ApiDocUrl { get; }

     public EntryPointModel(string version, string apiDocUrl)
     {
         Version = version;
         ApiDocUrl = apiDocUrl;
     }
 }
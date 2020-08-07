using NetFusion.Rest.Resources;

namespace WebTests.Rest.CodeGeneration.Server
{
    /// <summary>
    /// Example model for which code will be generated.
    /// </summary>
    [Resource("ResourceOne")]
    public class ApiModelOne
    {
        public string ModelOneProp { get; set; }
    }
}
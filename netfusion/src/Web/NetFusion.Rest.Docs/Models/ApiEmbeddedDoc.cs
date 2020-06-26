namespace NetFusion.Rest.Docs.Models
{
    public class ApiEmbeddedDoc
    {
        /// <summary>
        /// The name further describing what the embeeded resource represents.
        /// For example, the resource's name may be payment (identifyin the
        /// type of the embedded resource) but the embedded name could be
        /// 'last-payment'.
        /// </summary>
        public string[] EmbeddedNames { get; set; }

        /// <summary>
        /// Documentation associated with the embedded resource.
        /// </summary>
        public ApiResponseDoc ResponseDoc { get; set; } 
    }      
}
namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Class instance populated from the common-definition documentation file.
    /// This contains global documentation items that are used if not specified
    /// at the action level within the controller specific documentation file.
    /// </summary>
    public class CommonDefinitions
    {
        public CommonDefinitions()
        {
            HttpCodes = new DocHttpCode[] { };
            Relations = new DocRelation[] { };
            EmbeddedResources = new DocEmbeddedResource[] { };
        }

        public DocHttpCode[] HttpCodes { get; set; }
        public DocRelation[] Relations { get; set; }
        public DocEmbeddedResource[] EmbeddedResources { get; set; }
    }
}

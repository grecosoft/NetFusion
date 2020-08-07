namespace NetFusion.Rest.Docs.Entities
{
    /// <summary>
    /// Root entity containing HAL related comments.
    /// </summary>
    public class HalComments
    {
        public HalComments()
        {
            EmbeddedComments = new EmbeddedComment[] { };
            RelationComments = new RelationComment[] { };
        }
       
        /// <summary>
        /// Comments associated with a resource's embedded items.
        /// </summary>
        public EmbeddedComment[] EmbeddedComments { get; set; }
        
        /// <summary>
        /// Comments associated with a resource's link relations.
        /// </summary>
        public RelationComment[] RelationComments { get; set; }
    }
}
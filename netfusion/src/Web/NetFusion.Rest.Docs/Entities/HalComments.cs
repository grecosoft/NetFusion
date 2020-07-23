namespace NetFusion.Rest.Docs.Entities
{
    public class HalComments
    {
        public HalComments()
        {
            EmbeddedComments = new EmbeddedComment[] { };
            RelationComments = new RelationComment[] { };
        }
       
        public EmbeddedComment[] EmbeddedComments { get; set; }
        public RelationComment[] RelationComments { get; set; }
    }
}
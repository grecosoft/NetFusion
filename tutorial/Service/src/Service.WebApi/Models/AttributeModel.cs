using NetFusion.Rest.Resources;

namespace Service.WebApi.Models
{
    public class AttributeModel
    {
        public AttributeValue[] Attributes { get; set; }
    }


    public class AttributeValueBase
    {
        /// <summary>
        /// The last number in a sequence.
        /// </summary>
        public int LastSequenceNumber { get; set; }
    }
    
    
    /// <summary>
    /// Contains a name/value pair.
    /// </summary>
    [ExposedName("type-attribute")]
    public class AttributeValue : AttributeValueBase
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public string Value { get; set; }
    }
}
namespace Service.WebApi.Models
{
    public class AttributeModel
    {
        public AttributeValue[] Attributes { get; set; }
    }
    
    public class AttributeValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
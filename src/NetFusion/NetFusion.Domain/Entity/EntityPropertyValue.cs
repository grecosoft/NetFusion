namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Property that can be dynamically associated with a domain model.  
    /// This is for properties that are not static and can change or be
    /// configured over time.
    /// </summary>
    public class EntityAttributeValue
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}

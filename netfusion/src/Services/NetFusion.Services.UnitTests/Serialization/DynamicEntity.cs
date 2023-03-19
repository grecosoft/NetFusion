using NetFusion.Common.Base.Entity;

namespace NetFusion.Services.UnitTests.Serialization;

public class DynamicEntity : IAttributedEntity
{
    public IEntityAttributes Attributes { get; }
        
    public DynamicEntity()
    {
        Attributes = new EntityAttributes();
    }

    public bool IsActive { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }

    public IDictionary<string, object> AttributeValues
    {
        get => Attributes.GetValues();
        set => Attributes.SetValues(value);
    }
}
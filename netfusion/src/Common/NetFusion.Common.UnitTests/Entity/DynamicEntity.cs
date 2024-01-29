using NetFusion.Common.Base.Entity;

namespace NetFusion.Common.UnitTests.Entity;

public class DynamicEntity : IAttributedEntity
{
    public IEntityAttributes Attributes { get; } = new EntityAttributes();

    public bool IsActive { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }

    public IDictionary<string, object> AttributeValues
    {
        get => Attributes.GetValues();
        set => Attributes.SetValues(value);
    }
}
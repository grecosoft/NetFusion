﻿using FluentAssertions;
using NetFusion.Common.Base.Entity;

namespace NetFusion.Common.UnitTests.Entity;

public class AttributedEntityTests
{
    [Fact(DisplayName = "Can set Entity Attribute and Retrieve Dynamically")]
    public void CanSetEntityAttribute_And_RetrieveDynamically()
    {
        var entity = new DynamicEntity();
        entity.Attributes.SetValue("Value1", 1000);

        var value = entity.Attributes.Values.Value1 as object;
        value.Should().NotBeNull();
        value.Should().BeOfType<int>();

        var typedValue = (int)value;
        typedValue.Should().Be(1000);
    }

    [Fact(DisplayName = "Entity associated Attribute can be Deleted")]
    public void EntityAssociatedAttribute_CanBeDeleted()
    {
        var entity = new DynamicEntity();
        entity.Attributes.SetValue("Value1", 1000);

        var isDeleted = entity.Attributes.Delete("Value1");
        isDeleted.Should().BeTrue();

        isDeleted = entity.Attributes.Delete("Value1");
        isDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "Can Check if Entity has associated Attribute")]
    public void CanCheckIfEntity_HasAssociatedAttribute()
    {
        var entity = new DynamicEntity();
        entity.Attributes.SetValue("Value1", 1000);

        entity.Attributes.Contains("Value1").Should().BeTrue();
        entity.Attributes.Contains("Value2").Should().BeFalse();
    }

    [Fact(DisplayName = "Entity Attribute can be Retrieved as Specific Type")]
    public void EntityAttribute_CanBeRetrievedAsSpecificType()
    {
        var entity = new DynamicEntity();
        entity.Attributes.SetValue("Value1", 1000);

        var value = entity.Attributes.GetValue<int>("Value1");
        value.Should().Be(1000);
    }

    [Fact(DisplayName = "Can return types Default value if Attribute not present")]
    public void CanReturnTypesDefaultValue_IfAttributeNotPresent()
    {
        var entity = new DynamicEntity();
        var value = entity.Attributes.GetValueOrDefault("Value1", 0);
        value.Should().Be(0);
    }

    [Fact(DisplayName = "Can return specified Default Value if Attribute not present")]
    public void CanReturnSpecifiedDefaultValue_IfAttributeNotPresent()
    {
        var entity = new DynamicEntity();
        var value = entity.Attributes.GetValueOrDefault("Value1",  1000);
        value.Should().Be(1000);
    }

    [Fact(DisplayName = "Entity Attribute present Default value not returned")]
    public void EntityAttributePresent_DefaultValueNotReturned()
    {
        var entity = new DynamicEntity();
        entity.Attributes.Values.Value1 = 1000;
        var value = entity.Attributes.GetValueOrDefault("Value1", 2000);
        value.Should().Be(1000);
    }

    [Fact(DisplayName = "Calling Method Name used when name not specified")]
    public void CallingMethodNameUsed_WhenNameNotSpecified()
    {
        var entity = new DynamicEntity();
        entity.SetValueWithNoName(1000);

        var value = entity.GetValueWithNoName();
        value.Should().Be(1000);

        entity.AttributeValues.Should().HaveCount(1);
        entity.AttributeValues.First().Key.Should().Be("ValueWithNoName");
    }
}

public static class SampleContext
{
    public static int GetValueWithNoName(this IAttributedEntity entity)
    {
        return entity.Attributes.GetMemberValueOrDefault<int>();
    }

    public static void SetValueWithNoName(this IAttributedEntity entity, int value)
    {
        entity.Attributes.SetMemberValue(value);
    }
}
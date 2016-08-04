using FluentAssertions;
using NetFusion.Common.Serialization;
using NetFusion.Tests.Core.Domain.Mocks;
using Newtonsoft.Json;
using Xunit;

namespace NetFusion.Tests.Core.Domain
{
    public class EntitySerializationTests
    {
        [Fact]
        public void CanSerializeAndDeserializeAttributedEntity_Binary()
        {
            var entity = new DynamicEntity();

            entity.MaxValue = 1000; // static property.
            entity.Attributes.Values.Value1 = "ABC";  // dynamic attribute values
            entity.Attributes.Values.Value2 = 123;
            entity.Attributes.Values.Value3 = new[] { 10, 20 };

            var stream = BinaryFormatterUtility.Serialize(entity);
            var deserializedEntity = BinaryFormatterUtility.Deserialize<DynamicEntity>(stream);

            deserializedEntity.Should().NotBeNull();
            deserializedEntity.MaxValue.Should().Be(entity.MaxValue);

            var value1 = (string)deserializedEntity.Attributes.Values.Value1;
            var value2 = (int)deserializedEntity.Attributes.Values.Value2;
            var value3 = (int[])deserializedEntity.Attributes.Values.Value3;

            value1.Should().Be("ABC");
            value2.Should().Be(123);
            value3.Should().Equal(new int[] { 10, 20 });
        }

        [Fact]
        public void CanSerializeAndDeserializeAttributedEntity_JSON()
        {
            var entity = new DynamicEntity();

            entity.MaxValue = 1000; // static property.
            entity.Attributes.Values.Value1 = "ABC";  // dynamic attribute values
            entity.Attributes.Values.Value2 = 123;

            var jsonVal = JsonConvert.SerializeObject(entity);
            var deserializedEntity = JsonConvert.DeserializeObject<DynamicEntity>(jsonVal);

            deserializedEntity.Should().NotBeNull();
            deserializedEntity.MaxValue.Should().Be(entity.MaxValue);

            var value1 = (string)deserializedEntity.Attributes.Values.Value1;
            var value2 = (int)deserializedEntity.Attributes.Values.Value2;

            value1.Should().Be("ABC");
            value2.Should().Be(123);
        }

    }
}

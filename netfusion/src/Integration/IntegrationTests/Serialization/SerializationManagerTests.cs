namespace IntegrationTests.Serialization
{
    using System;
    using System.Text;
    using FluentAssertions;
    using NetFusion.Base;
    using NetFusion.Base.Serialization;
    using NetFusion.Serialization;
    using Xunit;

    public class SerializationManagerTests
    {
        [Fact]
        public void SerializersAre_AddedByDefault()
        {
            var mgr = new SerializationManager();
            mgr.Serializers.Should().HaveCount(2);

            mgr.Serializers.Should().Contain(s =>
                s.ContentType == ContentTypes.Json &&
                s.EncodingType == Encoding.UTF8.WebName);

            mgr.Serializers.Should().Contain(s => 
                s.ContentType == ContentTypes.MessagePack);
        }

        [Fact]    
        public void Serializers_Can_BeCleared()
        {
            var mgr = new SerializationManager();
            mgr.ClearSerializers();
            mgr.Serializers.Should().BeEmpty();
        }

        [Fact]
        public void Serializers_CanBeOverridden()
        {
            var mgr = new SerializationManager();
            mgr.AddSerializer(new MockSerializer());

            var serializer = mgr.GetSerializer(ContentTypes.Json, Encoding.UTF8.WebName);
            serializer.Should().BeOfType<MockSerializer>();
        }

        [Fact]
        public void IfEncodingTypeSpecified_RequiresExactMatch()
        {
            var mgr = new SerializationManager();

            Assert.Throws<InvalidOperationException>(
                () => mgr.GetSerializer(ContentTypes.Json, "MockEncoding"));
        }

        [Fact]
        public void IfEncodingTypeNotSpecified_MatchedByContentType()
        {
            var mgr = new SerializationManager();

            var serializer = mgr.GetSerializer(ContentTypes.Json);
            serializer.Should().BeOfType<JsonMessageSerializer>();
        }

        [Fact]
        public void ExceptionIfSerializer_NotResolved()
        {
            var mgr = new SerializationManager();

            Assert.Throws<InvalidOperationException>(
                () => mgr.GetSerializer("MockContentType"));
        }

        [Fact]
        public void EncodingType_Can_BeContainedIn_ContentType()
        {
            var mgr = new SerializationManager();

            var serializer = mgr.GetSerializer("application/json; charset=utf-8");
            serializer.Should().BeOfType<JsonMessageSerializer>();
        }

        private class MockSerializer : IMessageSerializer
        {
            public string ContentType => ContentTypes.Json;
            public string EncodingType => Encoding.UTF8.WebName;
            
            public byte[] Serialize(object value)
            {
                throw new NotImplementedException();
            }

            public object Deserialize(byte[] value, Type valueType)
            {
                throw new NotImplementedException();
            }

            public T Deserialize<T>(byte[] value, Type valueType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
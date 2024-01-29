using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Serialization;
using NetFusion.Services.Serialization;

namespace NetFusion.Services.UnitTests.Serialization;

public class SerializationManagerTests
{
    [Fact]
    public void DefaultSerializationManagerAdded_WhenRequested_WillBeUsed()
    {
        // Arrange:
        var registration = new ServiceCollection();
        registration.AddDefaultSerializationManager();
        
        // Act:
        var services = registration.BuildServiceProvider();
        
        // Assert:
        services.GetService<ISerializationManager>().Should().NotBeNull();
        services.GetService<ISerializationManager>().Should().BeOfType<SerializationManager>();
    }
    
    [Fact]
    public void CustomSerializationManagerAdded_WhenRequested_WillBeUsed()
    {
        // Arrange:
        var registration = new ServiceCollection();
        registration.AddSerializationManager<CustomSerializationManager>();
        
        // Act:
        var services = registration.BuildServiceProvider();
        
        // Assert:
        services.GetService<ISerializationManager>().Should().NotBeNull();
        services.GetService<ISerializationManager>().Should().BeOfType<CustomSerializationManager>();
    }
    
    [Fact]
    private void WhenResolved_SerializationManager_ReturnsSingletonInstance()
    {
        // Arrange:
        var registration = new ServiceCollection();
        registration.AddDefaultSerializationManager();
        
        // Act:
        var services = registration.BuildServiceProvider();
        
        // Assert:
        services.GetService<ISerializationManager>().Should()
            .BeSameAs(services.GetService<ISerializationManager>());
    }

    [Fact]
    public void Request_InvalidContentType_ExceptionRaised()
    {
        var serializationMgr = new SerializationManager();

        Assert.Throws<SerializationException>(
            () => serializationMgr.GetSerializer("InvalidContentType")
        ).ExceptionId.Should().Be("CONTENT_TYPE_NOT_FOUND");
    }

    [Fact]
    public void RequestWithoutEncoding_WhenMultipleContentTypeEncodings_RaisesException()
    {
        var serializationMgr = new CustomSerializationManager();
        serializationMgr.AddTwoSerializersForSameContentTypeDifferentEncoding();
        
        Assert.Throws<SerializationException>(
            () => serializationMgr.GetSerializer(ContentTypes.Json)
        ).ExceptionId.Should().Be("MULTIPLE_SERIALIZERS_FOUND");
    }

    [Fact]
    public void RequestWithEncoding_WhenMatchingContentTypeWrongEncoding_RaisesException()
    {
        var serializationMgr = new SerializationManager();

        Assert.Throws<SerializationException>(
            () => serializationMgr.GetSerializer(ContentTypes.Json, "InvalidEncoding")
        ).ExceptionId.Should().Be("ENCODING_NOT_FOUND");
    }

    [Fact]
    public void RequestWithEncoding_WhenMultipleContentTypeEncodings_ReturnsSerializer()
    {
        var serializationMgr = new CustomSerializationManager();
        serializationMgr.AddTwoSerializersForSameContentTypeDifferentEncoding();

        var serializer = serializationMgr.GetSerializer(ContentTypes.Json, Encoding.UTF8.WebName);
        serializer.Should().NotBeNull();
        serializer.Should().BeOfType<JsonMessageSerializer>();
    }

    [Fact]
    public void RequestWithEncoding_SpecifiedWithinContentType_ReturnsSerializer()
    {
        var serializationMgr = new CustomSerializationManager();
        serializationMgr.AddTwoSerializersForSameContentTypeDifferentEncoding();

        var serializer = serializationMgr.GetSerializer($"{ContentTypes.Json};charset={Encoding.UTF8.WebName}");
        serializer.Should().NotBeNull();
        serializer.Should().BeOfType<JsonMessageSerializer>();
    }

    [Fact]
    public void AddNewSerializer_WithMatchingContentTypeAndEncoding_OverridesExisting()
    {
        // Arrange:
        var serializationMgr = new CustomSerializationManager();

        var serializer = serializationMgr.GetSerializer(ContentTypes.Json);
        serializer.Should().NotBeNull();
        serializer.Should().BeOfType<JsonMessageSerializer>();
        
        // Act:
        var mockSerializer = new Mock<ISerializer>();
        mockSerializer.Setup(m => m.ContentType).Returns(ContentTypes.Json);
        mockSerializer.Setup(m => m.EncodingType).Returns(Encoding.UTF8.WebName);

        var newSerializer = mockSerializer.Object;
        serializationMgr.OverrideSerializer(newSerializer);
        
        // Assert:
        serializationMgr.GetSerializer(ContentTypes.Json, Encoding.UTF8.WebName)
            .Should().BeSameAs(newSerializer);
    }

    [Fact]
    public void SerializationRequest_ForValidContentType_ReturnsSerializedData()
    {
        var serializationMgr = new SerializationManager();
        var entity = new ExampleEntity("FirstValue", "SecondValue");

        var data = serializationMgr.Serialize(entity, ContentTypes.MessagePack);
        serializationMgr.Deserialize(ContentTypes.MessagePack, typeof(ExampleEntity), data)
            .Should().BeEquivalentTo(entity);
        
        serializationMgr.Deserialize<ExampleEntity>(ContentTypes.MessagePack, data)
            .Should().BeEquivalentTo(entity);
    }
    
    private class CustomSerializationManager : SerializationManager
    {
        public void AddTwoSerializersForSameContentTypeDifferentEncoding()
        {
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.ContentType).Returns(ContentTypes.Json);
            mockSerializer.Setup(m => m.EncodingType).Returns(Encoding.UTF32.WebName);
            
            AddSerializer(mockSerializer.Object);
        }

        public void OverrideSerializer(ISerializer serializer)
        {
            AddSerializer(serializer);
        }
    }

    public class ExampleEntity(string valueOne, string valueTwo)
    {
        public string ValueOne { get; } = valueOne;
        public string ValueTwo { get; } = valueTwo;
    }
  
}
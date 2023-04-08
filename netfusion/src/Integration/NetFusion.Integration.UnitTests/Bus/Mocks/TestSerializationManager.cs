using NetFusion.Common.Base.Serialization;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestSerializationManager : ISerializationManager
{
    public IEnumerable<ISerializer> Serializers { get; } = Array.Empty<ISerializer>();
    
    public ISerializer GetSerializer(string contentType, string? encodingType = null)
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize(object value, string contentType, string? encodingType = null)
    {
        throw new NotImplementedException();
    }

    public object? Deserialize(string contentType, Type valueType, byte[] value, string? encodingType = null)
    {
        throw new NotImplementedException();
    }

    public T? Deserialize<T>(string contentType, byte[] value, string? encodingType = null)
    {
        throw new NotImplementedException();
    }
}
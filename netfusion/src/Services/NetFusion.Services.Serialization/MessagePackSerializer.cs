using System;
using System.IO;
using MsgPack.Serialization;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Serialization;
using MsgPackCliSerializer = MsgPack.Serialization.MessagePackSerializer;

namespace NetFusion.Services.Serialization;

/// <summary>
/// Serializer for message pack serialization compact binary format.
/// </summary>
public class MessagePackSerializer : ISerializer
{        
    public string ContentType => ContentTypes.MessagePack;
    public string? EncodingType => null;
        
    public byte[] Serialize(object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        var serializer = MsgPackCliSerializer.Get(value.GetType());
        var memoryStream = new MemoryStream();
            
        serializer.Pack(memoryStream, value);
        return memoryStream.ToArray();
    }
    
    public object? Deserialize(byte[] value, Type valueType)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            
        var serializer = MsgPackCliSerializer.Get(valueType);
        var memoryStream = new MemoryStream(value);
        
        return serializer.Unpack(memoryStream);
    }
}
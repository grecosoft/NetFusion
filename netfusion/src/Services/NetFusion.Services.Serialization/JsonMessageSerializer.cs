using System;
using System.Text;
using System.Text.Json;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Serialization;

namespace NetFusion.Services.Serialization;

/// <summary>
/// Serializes a value to JSON representation. 
/// </summary>
public class JsonMessageSerializer : ISerializer
{
    public string ContentType => ContentTypes.Json;
    public string EncodingType => Encoding.UTF8.WebName;

    public byte[] Serialize(object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
            
        return JsonSerializer.SerializeToUtf8Bytes(value);
    }

    public object? Deserialize(byte[] value, Type valueType)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (valueType == null) throw new ArgumentNullException(nameof(valueType));

        return JsonSerializer.Deserialize(value, valueType);
    }
}
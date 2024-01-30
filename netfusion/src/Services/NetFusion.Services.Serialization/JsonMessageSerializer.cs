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
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.SerializeToUtf8Bytes(value);
    }

    public object? Deserialize(byte[] value, Type valueType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(valueType);

        return JsonSerializer.Deserialize(value, valueType);
    }
}
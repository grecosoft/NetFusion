using System;

namespace NetFusion.Common.Base.Serialization;

/// <summary>
/// Implementations determine how an object is serialized and deserialized.
/// Instances are registered with the ISerializationManager.
/// </summary>
public interface ISerializer 
{
    /// <summary>
    /// The content type (example: application/json).
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// The optional encoding of the serialized data.
    /// </summary>
    string? EncodingType { get; }

    /// <summary>
    /// Returns serialized representation of the object.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>Serialized object's bytes.</returns>
    byte[] Serialize(object value);

    /// <summary>
    /// Returns an object that has been deserialized from its serialized representation.
    /// </summary>
    /// <param name="value">Byte array containing serialized object.</param>
    /// <param name="valueType">The type of the object contained within the serialized byte array.</param>
    /// <returns>Instance of the deserialized object.</returns>
    object? Deserialize(byte[] value, Type valueType);
}
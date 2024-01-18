namespace NetFusion.Common.Base.Serialization;

/// <summary>
/// Interface implemented by message to provide custom deserialization.
/// </summary>
public interface ICustomDeserialize
{
    void Deserialize(byte[] value);
}
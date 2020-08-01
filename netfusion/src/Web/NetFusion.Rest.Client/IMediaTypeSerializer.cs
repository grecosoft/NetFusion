using System;
using System.IO;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Interface defining contract implemented by classes responsible for
    /// serializing and deserializing.
    /// </summary>
    public interface IMediaTypeSerializer
    {
        /// <summary>
        /// The media type supported by the serializer.
        /// </summary>
        string MediaType { get; }

        /// <summary>
        /// Serializes the provided value to a byte array.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>Byte array containing serialized object.</returns>
        byte[] Serialize(object value);

        /// <summary>
        /// Deserializes stream into an object instance.
        /// </summary>
        /// <typeparam name="T">The type of the object to be created from stream.</typeparam>
        /// <param name="responseStream">The stream containing the serialized contents.</param>
        /// <returns>Instance of object created from serialized stream contents.</returns>
        Task<T> Deserialize<T>(Stream responseStream);

        /// <summary>
        /// Deserializes stream into an object instance.
        /// </summary>
        /// <param name="responseStream">The stream containing the serialized contents.</param>
        /// <param name="type">he type of the object to be created from stream.</param>
        /// <returns>Instance of object created from serialized stream contents.</returns>
        Task<object> Deserialize(Stream responseStream, Type type);
    }
}
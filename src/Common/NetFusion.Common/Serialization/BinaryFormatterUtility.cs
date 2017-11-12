#if NET461

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace NetFusion.Common.Serialization
{
    public static class BinaryFormatterUtility
    {
        /// <summary>
        /// Serializes an object to binary representation.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>Byte array of binary data.</returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts byte array into its corresponding object instance.
        /// </summary>
        /// <param name="bytes">The type's serialized bytes.</param>
        /// <returns>Created object from serialized byte array.</returns>
        public static object Deserialize(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Converts byte array into its corresponding object instance.
        /// </summary>
        /// <typeparam name="T">Type of the serialized object.</typeparam>
        /// <param name="bytes">The type's serialized bytes.</param>
        /// <returns>Created object fro serialized byte array.</returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}

#endif
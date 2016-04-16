using Newtonsoft.Json;
using System.IO;

namespace NetFusion.Common.Serialization
{
    public static class JsonUtility
    {
        public static T Deserialize<T>(StreamReader stream)
        {
            var serializer = new JsonSerializer();
            return (T)serializer.Deserialize(stream, typeof(T));
        }
    }
}

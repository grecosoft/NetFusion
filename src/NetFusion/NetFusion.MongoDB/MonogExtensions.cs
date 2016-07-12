using MongoDB.Bson;

namespace NetFusion.MongoDB
{
    public static class MonogExtensions
    {
        /// <summary>
        /// If valid returns the ObjectId instance corresponding
        /// to the specified string value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>If the value is not a valid MongoDB value, an exception 
        /// is thrown.  Otherwise the corresponding ObjectId instance.</returns>
        public static ObjectId AsObjectId(this string value)
        {
            MongoCheck.ValidObjectId(value);
            return ObjectId.Parse(value);
        }
    }
}

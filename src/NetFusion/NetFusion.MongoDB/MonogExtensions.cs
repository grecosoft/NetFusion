using MongoDB.Bson;

namespace NetFusion.MongoDB
{
    public static class MonogExtensions
    {
        public static ObjectId AsObjectId(this string value)
        {
            MongoCheck.ValidObjectId(value);
            return ObjectId.Parse(value);
        }
    }
}

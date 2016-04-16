using MongoDB.Bson;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.MongoDB
{
    public class MongoCheck
    {
        public static void ValidObjectId(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(
                    nameof(value), "MongoDB ObjectId can't be null");
            }

            ObjectId objId;
            if (!ObjectId.TryParse(value, out objId))
            {
                throw new InvalidOperationException(
                    $"the value of {value} is not a valid string representation of an ObjectId");
            }
        }
    }
}

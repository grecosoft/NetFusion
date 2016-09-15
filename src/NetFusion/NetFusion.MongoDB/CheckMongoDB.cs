using MongoDB.Bson;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.MongoDB
{
    public class CheckMongoDB
    {
        /// <summary>
        /// Validates a string value containing a MongoDB key value.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        public static void ValidObjectId(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("MongoDB ObjectId string representation can't be null.");
            }

            ObjectId objId;
            if (!ObjectId.TryParse(value, out objId))
            {
                throw new InvalidOperationException(
                    $"The value of {value} is not a valid ObjectId string representation.");
            }
        }
    }
}

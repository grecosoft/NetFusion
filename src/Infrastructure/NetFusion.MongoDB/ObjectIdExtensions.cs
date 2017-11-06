using MongoDB.Bson;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.MongoDB
{
    public static class ObjectIdExtensions
    {
        /// <summary>
        /// Validates a string value containing a MongoDB key value.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        public static void ValidObjectId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("MongoDB ObjectId string representation can't be null.");
            }

            if (!ObjectId.TryParse(value, out ObjectId objId))
            {
                throw new InvalidOperationException(
                    $"The value of {value} is not a valid ObjectId string representation.");
            }
        }

        /// <summary>
        /// If valid returns the ObjectId instance corresponding
        /// to the specified string value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>If the value is not a valid MongoDB value, an exception 
        /// is thrown.  Otherwise the corresponding ObjectId instance.</returns>
        public static ObjectId AsObjectId(this string value)
        {
            ValidObjectId(value);
            return ObjectId.Parse(value);
        }
    }
}

using System;

namespace NetFusion.Common.Extensions
{
    /// <summary>
    /// String extensions.
    /// </summary>
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }
    }
}

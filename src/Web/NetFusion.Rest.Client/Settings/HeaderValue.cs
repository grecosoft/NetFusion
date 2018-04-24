using System;

namespace NetFusion.Rest.Client.Settings
{
    /// <summary>
    /// Contains a value to be sent as a request HTTP Header value.
    /// </summary>
    public class HeaderValue
    {
        /// <summary>
        /// The header value.
        /// </summary>
        public string[] Value { get; }

		/// <summary>
		/// The precedence of the value compared in relation to other values with the same header name.
		/// </summary>
		public double? Quality { get; }

        private HeaderValue(string[] value, double? quality)
        {
			if (value == null || value.Length == 0)
			{
				throw new ArgumentException("At least one header value required.", nameof(value));
			}

			if (quality != null && quality < 0)
			{
				throw new ArgumentException("Quality value must be greater than zero.", nameof(quality));
			}

            Value = value;
            Quality = quality;
        }

        public static HeaderValue WithValue(string[] value, double? quality = null)
        {
            return new HeaderValue(value, quality);
        }

        public static HeaderValue WithValue(string value, double? quality = null)
        {
            return new HeaderValue(new[] { value }, quality);
        }     
    }
}

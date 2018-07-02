using System;
using System.Collections.Generic;

namespace Nougat.Display
{
    /// <summary>
    /// Represents key/value pairs for a given row of data.
    /// </summary>
    public class DataRow
    {
        // Allows the caller to specify a delegate to return a list
        // of strings to be displayed for a given row's details.
        public Func<IEnumerable<string>> Details { get; }
        public IDictionary<string, string> Values { get; } 

        public DataRow()
        {
            Details = () => Array.Empty<string>();
            Values = new Dictionary<string, string>();
        }

        public DataRow(Func<IEnumerable<string>> details) : this()
        {
           Details = details ?? throw new ArgumentNullException(nameof(details));
        }
    }
}
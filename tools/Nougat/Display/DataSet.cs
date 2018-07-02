using System;

namespace Nougat.Display
{
    /// <summary>
    /// Represents a set of data rows to be displayed as a table.
    /// </summary>
    public class DataSet
    {
        public DataRow[] Rows { get; }

        public DataSet(DataRow[] rows)
        {
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }
    }
}
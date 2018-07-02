using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Nougat.Display
{
    /// <summary>
    /// Displays a populated data set as a formatted table.
    /// </summary>
    public class DataSetViewer
    {
        public static void Display(string caption, DataSet dataSet, TextWriter writer)
        {
            if (caption == null) throw new ArgumentNullException(nameof(caption));
            if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var columns = BuildDisplayColumns(dataSet);
            var strBuilder = new StringBuilder();
            
            DisplayHeader(strBuilder, caption, columns);
            DisplayResults(strBuilder, columns, dataSet);
        
            var displayResult = strBuilder.ToString();
            writer.WriteLine(displayResult);
        }

        private static DisplayColumn[] BuildDisplayColumns(DataSet dataSet)
        {
            // Determines column name and the longest value in that column.
            var columns = dataSet.Rows.SelectMany(d => d.Values)
                .GroupBy(i => i.Key)
                .Select(g => new DisplayColumn { 
                    Name = g.Key, 
                    MaxValueWidth = g.Max(i => i.Value.Length)
                }).ToArray();

            // Make sure the column name is not larger that the data.
            foreach (var col in columns)
            {
                if (col.MaxValueWidth < col.Name.Length)
                {
                    col.MaxValueWidth = col.Name.Length;
                }
            }

            return columns;
        }

        private static void DisplayHeader(StringBuilder strBuilder, string caption, DisplayColumn[] columns)
        {
            int tableWidth = columns.Sum(c => c.ColumnTotalWidth) - DisplayColumn.ColPadding;

            strBuilder.AppendLine(new string('=', tableWidth));
            strBuilder.AppendLine(caption);
            strBuilder.AppendLine(new string('=', tableWidth));
            
            foreach(var column in columns)
            {
                strBuilder.Append(column.Name.PadRight(column.MaxValueWidth));
                strBuilder.Append(new string(' ', DisplayColumn.ColPadding));
            }

            strBuilder.AppendLine();

            foreach(var column in columns)
            {
                strBuilder.Append(new string('-', column.MaxValueWidth));
                strBuilder.Append(new string(' ', DisplayColumn.ColPadding));
            }
            
            strBuilder.AppendLine();
        }

        private static void DisplayResults(StringBuilder strBuilder, 
            DisplayColumn[] columns, 
            DataSet dataSet)
        {
            if (!dataSet.Rows.Any())
            {
                strBuilder.AppendLine("No Resources to list.");
            }
            
            foreach (DataRow dataItem in dataSet.Rows)
            {
                foreach(DisplayColumn columnItem in columns)
                {
                    strBuilder.Append(
                        dataItem.Values[columnItem.Name].PadRight(columnItem.MaxValueWidth));

                    strBuilder.Append(new string(' ', DisplayColumn.ColPadding));
                }

                strBuilder.AppendLine();/// <summary>
    /// Manages how different models are rendered.
    /// </summary>

                // Optional Details...
                var details = dataItem.Details();
                foreach (string detail in details)
                {
                    strBuilder.Append(new string(' ', DisplayColumn.ColPadding));
                    strBuilder.AppendLine(detail);
                }

                if (details.Any())
                {
                    strBuilder.AppendLine();
                }
            }
        }
    }
}
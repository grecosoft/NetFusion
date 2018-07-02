namespace Nougat.Display
{
    /// <summary>
    /// Represents a column of data to be displayed.
    /// </summary>
    public class DisplayColumn
    {
        public const int ColPadding = 3;
        
        /// <summary>
        /// The name of the column.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The max width of the value contained in the column.
        /// </summary>
        public int MaxValueWidth { get; set;}
        
        /// <summary>
        /// Total column width including padding.
        /// </summary>
        public int ColumnTotalWidth => MaxValueWidth + ColPadding;
    }
}
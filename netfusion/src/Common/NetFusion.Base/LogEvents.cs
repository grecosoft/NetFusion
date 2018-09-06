namespace NetFusion.Base
{
    /// <summary>
    /// Base log event numbers for each implementation category.  Within each category, 
    /// sub-categories are created by adding multiple of 100 to these base numbers.  
    /// Then within each sub-category, groups of events can be declared by adding multiples 
    /// of 20 to the base sub-category value.  Within a given sub-category, exceptions have
    /// negative values decreasing by one starting at the base sub-category value.
    /// </summary>
    public class LogEvents
    {
        public const int Common = 1000;
        public const int Core = 2000;
        public const int Integration = 3000;
        public const int Web = 4000;
    }
}

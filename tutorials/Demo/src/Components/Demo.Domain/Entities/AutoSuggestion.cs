namespace Demo.Domain.Entities
{
    public class AutoSuggestion
    {
        public string Make { get; }
        public string Model { get; }
        public int Year { get; }
        public string Color { get; }

        public AutoSuggestion(string make, string model,
            int year,
            string color)
        {
            Make = make;
            Model = model;
            Year = year;
            Color = color;
        }
    }
}
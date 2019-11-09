using NetFusion.Rest.Resources.Hal;
using Demo.Domain.Entities;

namespace Demo.WebApi.Resources
{
    public class AutoResource : HalResource
    {
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }

        public static AutoResource FromEntity(AutoSuggestion entity)
        {
            return new AutoResource
            {
                Make = entity.Make,
                Model = entity.Model,
                Color = entity.Color,
                Year = entity.Year
            };
        }
    }
}
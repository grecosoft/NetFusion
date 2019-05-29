using NetFusion.Rest.Resources.Hal;

namespace Service.WebApi.Resources
{
    public class SchoolResource : HalResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberTeachers { get; set; }
        public int YearEstablished { get; set; }
    }
}
using System;
using NetFusion.Rest.Resources;

namespace Service.WebApi.Resources
{
    public class StudentResource : HalResource
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
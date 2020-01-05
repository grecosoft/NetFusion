using System;
using NetFusion.Rest.Resources.Hal;

namespace Demo.WebApi.Resources
{
    public class PriceHistoryModel
    {
        public int PriceHistoryId { get; set; }
        public int ListingId { get; set; }
        public DateTime DateOfEvent { get; set; }
        public string Event { get; set; }
        public decimal Price { get; set; }
        public string Source { get; set; }
    }
}


//public class Student
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//}
//
//public class Class
//{
//    public string Name { get; set; }
//    public int NumberStudents { get; set; }
//    public string Instructor { get; set; }
//}

//public class Test
//{
//    public void Code()
//    {
//        var studentRes = new Student
//        {
//            FirstName = "John",
//            LastName = "Smith"
//        }.AsResource();
//
//        var classModels = new[]
//        {
//            new Class
//            {
//                Name = "Fixing 15 Year Old Monolithic Applications",
//                Instructor = "Mr. Hammer",
//                NumberStudents = 1
//            },
//            new Class
//            {
//                Name = "Software Design and Architecture",
//                Instructor = "Mr. Dry",
//                NumberStudents = 10
//            }
//        };
//        
//        studentRes.Embed(classModels, "required-classes");
//    }
//}
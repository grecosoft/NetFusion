using System.Linq;
using Demo.Domain.Entities;

namespace Demo.App.Services
{
    public class SampleEntityService
    {
        private static Course ExampleEntity =>
            new Course("Computer Science",
                    "Mark Smith",
                    2018,
                    2);

        private static void AddStudents(Course course)
        {
            course.AddStudent(
                new Student("Tom", "Green", new[] { 50, 33 }));

            course.AddStudent(
                new Student("Jim", "Smith", new[] { 90, 89 }));
        }

        // Methods returning entity for use by mappings examples:
        public Course GetCourse() 
        {
            var course = ExampleEntity;

            AddStudents(course);
            return course;
        }
        
        public object[] GetContacts() 
        {
            var entity = ExampleEntity;
            AddStudents(entity);

            var student = entity.Students.First();
            var teacher = new Teacher
            {
                FirstName = "Ben",
                LastName = "Smith",
                City = "New Kensington",
                State = "PA",
                Zip = "15068"
            };

            return new object[]
            {
                student,
                teacher
            };
        }
        
        public Student GetStudent()
        {
            var entity = ExampleEntity;
            AddStudents(entity);

            var student = entity.Students.First();
            student.Attributes.Values.MaxScore = student.Scores.Max();
            student.Attributes.Values.MinScore = student.Scores.Min();

            return student;
        }
        
        public Car GetCar()
        {
            return new Car {
                Make = "Honda",
                Model = "Accord",
                Year = 2010,
                Price = 8000,
                WasSmokerCar = true,
                HasSalvageTitle = true
            };
        }
    }
}

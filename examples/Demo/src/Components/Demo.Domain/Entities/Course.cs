using System.Collections.Generic;

namespace Demo.Domain.Entities
{
    public class Course
    {
        public string Name { get; }
        public string Instructor { get; }
        public int Year { get; }
        public int Semester { get; set; }
        public IReadOnlyCollection<Student> Students => _students;

        private List<Student> _students;


        public Course(
            string name,
            string instructor,
            int year,
            int semester)
        {
            this.Name = name;
            this.Instructor = instructor;
            this.Year = year;
            this.Semester = semester;
            _students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            _students.Add(student);
        }
    }
}

using System;
namespace WebApiHost.Models
{
    public class StudentListingSummary
    {
    public string Instructor { get; set; }
    public string Name { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
        public int HighestScore { get; set; }
        public StudentAverage[] Averages { get; set; }
    }

    public class StudentAverage
    {
        public string Name { get; set; }
        public double AverageScore { get; set; }
    }
}

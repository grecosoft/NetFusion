using System.Linq;
using Demo.App.Entities;
using WebApiHost.Models;
using NetFusion.Mapping;

namespace WebApiHost.Mappings
{
    public class StudentListMapping : MappingStrategy<Course, StudentListingSummary>
    {
        protected override StudentListingSummary SourceToTarget(Course source)
        {
            return new StudentListingSummary {

                Instructor = source.Instructor,
                Name = source.Name,
                Year = source.Year,
                Semester = source.Semester,

                HighestScore = source.Students
                    .SelectMany(s => s.Scores)
                    .Max(),

                Averages = source.Students
                    .Select(s => new StudentAverage
                    {
                        Name = s.FirstName + s.LastName,
                        AverageScore = s.Scores.Average()
                    })
                    .ToArray()
            };
        }
    }
}

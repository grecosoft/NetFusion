using System.Linq;
using Demo.App.Entities;
using NetFusion.Mapping;
using WebApiHost.Models;

namespace WebApiHost.Mappings
{
    public class StudentContactMapping : MappingStrategy<Student, StudentSummary>
    {
        protected override StudentSummary SourceToTarget(Student source)
        {
            return new StudentSummary
            {
                FullName = source.FirstName + " == " + source.LastName,
                MaxScore = source.Scores.Max(),
                MinScore = source.Scores.Min()
            };
        }
    }
}

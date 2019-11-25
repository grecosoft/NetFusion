using System.Linq;
using Demo.Domain.Entities;
using NetFusion.Mapping;
using Demo.WebApi.Models;

namespace Demo.WebApi.Mappings
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

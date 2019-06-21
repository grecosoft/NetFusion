using Demo.Domain.Entities;
using NetFusion.Mapping;
using Demo.WebApi.Models;

namespace Demo.WebApi.Mappings
{
    public class TeacherContactMapping : MappingStrategy<Teacher, TeacherSummary>
    {
        protected override TeacherSummary SourceToTarget(Teacher source)
        {
            return new TeacherSummary
            {
                FullName = source.FirstName + " " + source.LastName,
                State = source.State,
                Zip =  source.Zip
            };
        }
    }
}

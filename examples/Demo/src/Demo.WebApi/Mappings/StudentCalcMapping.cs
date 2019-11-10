using NetFusion.Mapping;
using Demo.Domain.Entities;
using Demo.Domain.Services;
using Demo.WebApi.Models;

namespace Demo.WebApi.Mappings
{
    public class StudentCalcMapping : MappingStrategy<Student, StudentCalcSummary>
    {
        private IEntityIdGenerator IdGenerator { get; }

        public StudentCalcMapping(IEntityIdGenerator idGenerator)
        {
            this.IdGenerator = idGenerator;
        }

        protected override StudentCalcSummary SourceToTarget(Student source)
        {
            var summary = new StudentCalcSummary
            {
                StudentId = IdGenerator.GenerateId(),
                FullName = source.FirstName + " " + source.LastName,
                Calculations = source.AttributeValues
            };

            return summary;
        }
    }
}

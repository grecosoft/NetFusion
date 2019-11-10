using System.Collections.Generic;

namespace Demo.WebApi.Models
{
    public class StudentCalcSummary
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public IDictionary<string, object> Calculations { get; set; }
    }
}

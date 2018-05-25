using System.Collections.Generic;

namespace WebApiHost.Models
{
    public class StudentCalcSummary
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public IDictionary<string, object> Calculations { get; set; }
    }
}

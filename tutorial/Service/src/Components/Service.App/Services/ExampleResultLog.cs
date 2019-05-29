using System.Collections.Generic;
using Service.Domain.Services;

namespace Service.App.Services
{
    public class ExampleResultLog : IExampleResultLog
    {
        private readonly List<string> _logs = new List<string>();

        public string[] GetLogs() => _logs.ToArray();
        
        public void AddResult(string value)
        {
            _logs.Add(value);
        }
    }
}
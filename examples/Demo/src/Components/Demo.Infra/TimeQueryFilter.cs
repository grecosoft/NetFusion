using System;
using System.Threading.Tasks;
using Demo.Domain.Queries;
using NetFusion.Base.Entity;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types;

namespace Demo.Infra
{
    public class TimeQueryFilter :
        IPreQueryFilter,
        IPostQueryFilter
    {
        private ITimestamp _queryWithTimestamp;
        
        public Task OnPreExecute(IQuery query)
        {
            if (query is ITimestamp timestamp)
            {
                timestamp.CurrentDate = DateTime.UtcNow;
                _queryWithTimestamp = timestamp;
            }

            return Task.CompletedTask;
        }

        public Task OnPostExecute(IQuery query)
        {
            if (_queryWithTimestamp == null) return Task.CompletedTask;
            
            if (query.Result is IAttributedEntity attributed)
            {
                attributed.Attributes.Values.DayOfWeek = _queryWithTimestamp.CurrentDate.DayOfWeek;
            }
            return Task.CompletedTask;
        }
    }
}
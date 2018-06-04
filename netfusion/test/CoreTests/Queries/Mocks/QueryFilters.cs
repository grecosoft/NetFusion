﻿using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types;
using System.Threading.Tasks;

namespace CoreTests.Queries.Mocks
{
    public class QueryFilterOne : IPreQueryFilter, IPostQueryFilter
    {
        public Task OnPreExecute(IQuery query)
        {
            if (query is TestQuery testQuery)
            {
                testQuery.TestLog.Add("QueryFilterOne-Pre");
            }

            return Task.CompletedTask;
        }

        public Task OnPostExecute(IQuery query)
        {
            if (query is TestQuery testQuery)
            {
                testQuery.TestLog.Add("QueryFilterOne-Post");
            }

            return Task.CompletedTask;
        }
    }

    public class QueryFilterTwo : IPreQueryFilter, IPostQueryFilter
    {
        public Task OnPreExecute(IQuery query)
        {
            if(query is TestQuery testQuery)
            {
                testQuery.TestLog.Add("QueryFilterTwo-Pre");
            }

            return Task.CompletedTask;
        }

        public Task OnPostExecute(IQuery query)
        {
            if (query is TestQuery testQuery)
            {
                testQuery.TestLog.Add("QueryFilterTwo-Post");
            }

            return Task.CompletedTask;
        }
    }
}
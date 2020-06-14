using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Web.Mvc.Metadata
{
    public class ApiResponseMeta
    {
        public Type ModelType { get; set; }
        public int[] Statuses { get; }

        public ApiResponseMeta(IEnumerable<int> statuses, Type modelType)
        {
            ModelType = modelType == typeof(void) ? null : modelType;
            Statuses = statuses.ToArray();
        }
    }
}
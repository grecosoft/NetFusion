using System;

namespace NetFusion.Web.Mvc.Metadata
{
    public class ApiResponseMeta
    {
        public Type ModelType { get; set; }
        public int Status { get; }

        public ApiResponseMeta(int status, Type modelType)
        {
            ModelType = modelType == typeof(void) ? null : modelType;
            Status = status;
        }
    }
}
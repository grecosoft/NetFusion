using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Exceptions;
using NetFusion.Common.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace NetFusion.WebApi.Filters
{
    public class NetFusionExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            IContainerLogger logger = AppContainer.Instance.Logger;
            Exception exception = actionExecutedContext.Exception;

            if (exception.GetType() == typeof(UnauthorizedAccessException))
            {
                SetResponseAndLog(actionExecutedContext, "Access to the Web API is not authorized.", 
                    HttpStatusCode.Unauthorized);
                return;
            }

            var netFusionEx = actionExecutedContext.Exception as NetFusionException;
            if (netFusionEx != null && logger.IsDebugLevel)
            {
                var errContent = new
                {
                    netFusionEx.Message,
                    Details = netFusionEx.Details
                };

                var errMessage = errContent.ToIndentedJson();

                SetResponseAndLog(actionExecutedContext, errMessage, 
                    HttpStatusCode.InternalServerError);
                return;
            }

            SetResponseAndLog(actionExecutedContext, "Unexpected Exception", 
                HttpStatusCode.InternalServerError);
        }

        private void SetResponseAndLog(HttpActionExecutedContext context, string message, HttpStatusCode status)
        {
            AppContainer.Instance.Logger.Error(message);

            context.Response = new HttpResponseMessage()
            {

                Content = new StringContent(message, System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = status
            };

            base.OnException(context);
        }
    }
}

using NetFusion.Bootstrap.Logging;
using NetFusion.Logging.Serilog;
using NetFusion.WebApi.Metadata;
using Serilog;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    [EndpointMetadata(EndpointName = "NetFusion.Logging", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/logging")]
    public class LoggingController : ApiController
    {
        private readonly IContainerLogger _containerLogger;
        private readonly ILogger _logger;

        public LoggingController(IContainerLogger containerLogger, ILogger logger)
        {
            _containerLogger = containerLogger;
            _logger = logger;
        }

        [HttpPost, Route("plugin-log", Name = "CreateTestPluginLog")]
        public void CreateTestPluginLog()
        {

            _containerLogger.ForContext<LoggingController>().Debug("--container logger", new { a = 10 });
          
            var logger2 = _logger.ForContextType<LoggingController>();
            logger2.Debug("SDFSDFSDF");


        }
    }
}
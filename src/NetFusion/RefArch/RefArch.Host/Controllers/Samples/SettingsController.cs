using NetFusion.WebApi.Metadata;
using RefArch.Domain.Samples.Settings;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    /// <summary>
    /// A plug-in defines application settings by deriving a POCO from the base AppSetings class.  
    /// This class contains properties with settings specific to the plug-in defining the settings 
    /// class.  Multiple application-settings classes can be defined by a plug-in.  
    /// </summary>
    [EndpointMetadata(EndpointName = "NetFusion.Settings", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/settings")]
    public class SettingsController : ApiController
    {
        private readonly UninitializedSettings _unitializedSettings;
        private readonly InitializedSettings _initializedSettings;
        private readonly FileInitializedSettings _fileInitializedSettings;
        private readonly MachineFileInitializedSettings _machineFileInitializedSettings;
        private readonly MongoInitializedSettings _mongoInitializedSettings;

        public SettingsController(
            UninitializedSettings unitializedSettings,
            InitializedSettings initializedSettings,
            FileInitializedSettings fileInitializedSettings,
            MachineFileInitializedSettings machineFileInitializedSettings,
            MongoInitializedSettings mongoInitializedSettings)
        {
            _unitializedSettings = unitializedSettings;
            _initializedSettings = initializedSettings;
            _fileInitializedSettings = fileInitializedSettings;
            _machineFileInitializedSettings = machineFileInitializedSettings;
            _mongoInitializedSettings = mongoInitializedSettings;
        }

        /// <summary>
        /// If a setting specifies IsInitializationRequired to be false, the settings
        /// can be used without having an IAppSettingsInitializer defined.
        /// 
        /// http://localhost:54164/api/netfusion/samples/settings/uninitialized
        /// 
        /// 1.  Create application settings class: <see cref="UninitializedSettings"/>
        /// 2.  Set IsInitializationRequired = false in the setting's class constructor.
        /// 3.  Dependency-Inject it into depending class. 
        /// </summary>
        /// <returns>Configuration</returns>
        [HttpGet, Route("uninitialized", Name = "GetUninitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public UninitializedSettings GetUninitializedSettings()
        {
            return _unitializedSettings;
        }

        /// <summary>
        /// If a setting specifies IsInitializationRequired to be true (the default value),
        /// the settings must have a corresponding IAppSettingsInitializer.  If a settings
        /// initializer is not found, an exception is raised.
        /// 
        /// http://localhost:54164/api/netfusion/samples/settings/initialized
        /// 
        /// The host or application centric plug-in can define an initializer for a defined 
        /// application settings class.  Initializers can be for a specific application settings
        /// class (closed-generic) or applicable to any application setting (open-generic).  If a
        /// settings specific initializer is found and is able to initialize the setting, it is used.
        /// If there is no defined specific initializer or one is defined and it can't satisfy the
        /// request, all open-generic initializers are processed.  The first found open-generic 
        /// initializer (in configuration order) that can satisfy the request will be used.
        ///
        /// 1.  Create application settings class: <see cref="InitializedSettings"/>
        /// 2.  Set IsInitializationRequired = true in the setting's class constructor.
        /// 3.  Create an IAppSettingsInitializer based on the setting's type.
        /// 4.  Dependency-Inject it into depending class. 
        /// </summary>
        /// <returns>Configuration</returns>
        [HttpGet, Route("initialized", Name = "GetInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public InitializedSettings GetInitializedSettings()
        {
            return _initializedSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("file-initialized", Name = "GetFileInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public FileInitializedSettings GetFileInitializedSettings()
        {
            return _fileInitializedSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("machine-file-initialized", Name = "GetMachineFileInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public MachineFileInitializedSettings GetMachineFileInitializedSettings()
        {
            return _machineFileInitializedSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("mongo-initialized", Name = "GetMongoInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public MongoInitializedSettings GetMongoInitializedSettings()
        {
            return _mongoInitializedSettings;
        }
    }
}
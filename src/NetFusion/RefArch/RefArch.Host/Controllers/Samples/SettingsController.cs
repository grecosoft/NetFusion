using NetFusion.WebApi.Metadata;
using RefArch.Domain.Samples.MongoDb;
using RefArch.Domain.Samples.Settings;
using System.Threading.Tasks;
using System.Web.Http;

namespace RefArch.Host.Controllers.Samples
{
    /// <summary>
    /// A plug-in defines application settings by deriving a POCO from the base AppSetings class.  
    /// This class contains properties with settings specific to the host application or plug-in
    /// defining the settings class.  Multiple application-settings classes can be defined by a 
    /// plug-in.  IAppSettingsInitializer implementations determine how the application settings
    /// classes are populated.  The settings classes are initialized when injected into a dependent
    /// component for the first time.  The setting classes are registered within the container as 
    /// singletons. 
    /// </summary>
    [EndpointMetadata(EndpointName = "NetFusion.Settings", IncluedAllRoutes = true)]
    [RoutePrefix("api/netfusion/samples/settings")]
    public class SettingsController : ApiController
    {
        private readonly ISettingsInitService _settingsInitSrv;
        private readonly UninitializedSettings _unitializedSettings;
        private readonly InitializedSettings _initializedSettings;
        private readonly FileInitializedSettings _fileInitializedSettings;
        private readonly MachineFileInitializedSettings _machineFileInitializedSettings;
        private readonly MongoInitializedSettings _mongoInitializedSettings;

        public SettingsController(
            ISettingsInitService settingsInitsrv,
            UninitializedSettings unitializedSettings,
            InitializedSettings initializedSettings,
            FileInitializedSettings fileInitializedSettings,
            MachineFileInitializedSettings machineFileInitializedSettings,
            MongoInitializedSettings mongoInitializedSettings)
        {
            _settingsInitSrv = settingsInitsrv;
            _unitializedSettings = unitializedSettings;
            _initializedSettings = initializedSettings;
            _fileInitializedSettings = fileInitializedSettings;
            _machineFileInitializedSettings = machineFileInitializedSettings;
            _mongoInitializedSettings = mongoInitializedSettings;
        }

        [HttpPost, Route("init-mongo", Name = "InitMongoSettings")]
        public Task<MongoInitializedSettings> InitMongoSettings()
        {
            return _settingsInitSrv.InitMongoDbStoredSettings();
        }

        /// <summary>
        /// If a setting specifies IsInitializationRequired to be false, the settings
        /// can be used without having an IAppSettingsInitializer defined.
        /// 
        /// 1.  Create application settings class: <see cref="UninitializedSettings"/>
        /// 2.  Set IsInitializationRequired = false in the setting's class constructor.
        /// 3.  Dependency-Inject it into the dependent class.  
        /// </summary>
        /// <returns>Settings</returns>
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
        /// The host or application centric plug-in can define an initializer for a defined 
        /// application settings class.  Initializers can be for a specific application settings
        /// class (closed-generic) or applicable to any application setting (open-generic).  If a
        /// settings specific initializer is found and is able to initialize the setting, it is used.
        /// If there is no defined specific initializer or one is defined and but can't satisfy the
        /// request, all open-generic initializers are processed.  The first found open-generic 
        /// initializer (in configuration order) that can satisfy the request will be used.
        ///
        /// 1.  Create application settings class: <see cref="InitializedSettings"/>
        /// 2.  Set IsInitializationRequired = true in the setting's class constructor.
        /// 3.  Create an IAppSettingsInitializer based on the setting's type.
        /// 4.  Dependency-Inject settings into depending class. 
        /// </summary>
        /// <returns>Settings</returns>
        [HttpGet, Route("initialized", Name = "GetInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public InitializedSettings GetInitializedSettings()
        {
            return _initializedSettings;
        }

        /// <summary>
        /// The NetFusion.Settings plug-in contains an initializer named FileSettingsInitializer.  
        /// This implementation will load a settings class from a JSON file contained on disk.  It
        /// is suggested to register the FileSettings initializer first.  This allows local defined
        /// settings to override externally stored setting values.  This can be used when developing
        /// a plug-in. 
        /// 
        /// 1.  Create application settings class: <see cref="FileInitializedSettings"/>
        /// 2.  Register the FileSettingsInitializer
        /// <code>
        /// .WithConfig((NetFusionConfig config) => {
        ///
        ///            config.AddSettingsInitializer(
        ///                typeof(FileSettingsInitializer<>),
        ///                typeof(MongoSettingsInitializer<>));
        ///        })
        /// </code>
        /// 3.  Add settings values to JSON file in the following solution directory:  
        ///     Configs\Dev\FileInitializedSettings.json  Settings can be specified by
        ///     environment.  The name of the file matches the setting's class name.
        /// 4.  Dependency-Inject settings into depending class.
        /// </summary>
        /// <returns>Settings</returns>
        [HttpGet, Route("file-initialized", Name = "GetFileInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public FileInitializedSettings GetFileInitializedSettings()
        {
            return _fileInitializedSettings;
        }

        /// <summary>
        /// The FileSettingsInitializer will first check if there is a settings
        /// file for the specific computer on which the application is running.
        /// This allows developers to override settings specifically for their
        /// development.  For the development environment, the directory would 
        /// be:  Configs\COMP-NAME\Dev\FileInitializedSettings.json
        /// </summary>
        /// <returns>Settings</returns>
        [HttpGet, Route("machine-file-initialized", Name = "GetMachineFileInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public MachineFileInitializedSettings GetMachineFileInitializedSettings()
        {
            return _machineFileInitializedSettings;
        }

        /// <summary>
        /// The NetFusion.Settings.MongoDB plug-in provides a file settings initializer
        /// that will load values stored in a MongoDB document.  Each settings class has
        /// its values stored in a document.  The document must have the ApplicationId
        /// set the values specified within the manifest of type IAppHostPluginManifest.
        /// The document's Environment property needs to specify the environment and the
        /// MachineName can be optionally set.
        /// </summary>
        /// <returns>Settings</returns>
        [HttpGet, Route("mongo-initialized", Name = "GetMongoInitializedSettings")]
        [RouteMetadata(IncludeRoute = true)]
        public MongoInitializedSettings GetMongoInitializedSettings()
        {
            return _mongoInitializedSettings;
        }
    }
}
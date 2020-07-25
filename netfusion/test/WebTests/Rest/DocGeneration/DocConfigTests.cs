using Xunit;

namespace WebTests.Rest.DocGeneration
{
    /// <summary>
    /// Tests that validation the correct configuration of the plugin.
    /// </summary>
    public class DocConfigTests
    {
        /// <summary>
        /// By default, the location containing any additional files used to
        /// add addition documentations to the returned document model are
        /// contained within the application host's base directory.  This
        /// directory, for example, can contain VS C# Code comment XML files.
        /// </summary>
        [Fact]
        public void ByDefaultDescriptionFiles_LocatedInBaseDirectory()
        {
            
        }

        /// <summary>
        /// When the plugin is bootstrapped, the location of description
        /// files can be specified.
        /// </summary>
        [Fact]
        public void BootstrapCan_Specify_FileDescriptionDirectory()
        {
            
        }

        /// <summary>
        /// A middleware component exposes an endpoint called by clients to
        /// obtain documentation for a given WebApi controller method.
        /// </summary>
        [Fact]
        public void DocumentationEndpoint_HasDefaultValue()
        {
            
        }

        /// <summary>
        /// During the bootstrap process, the default documentation endpoint
        /// can be overriden.
        /// </summary>
        [Fact]
        public void BootstrapCan_Specify_DocumentationEndPoint()
        {
            
        }

        /// <summary>
        /// By default, Camel Case serialization settings are used.
        /// </summary>
        [Fact]
        public void DefaultSerializationSettingsSpecified()
        {
            
        }

        /// <summary>
        /// When bootstrapping, the default serialization settings can
        /// be overriden with custom settings.
        /// </summary>
        [Fact]
        public void SerializationSetting_CanBeSpecified()
        {
            
        }

        /// <summary>
        /// As the Action Document Model is being created, it will invoke
        /// configured classes deriving from IDocDescription.  These classes
        /// are responsible for adding details to the created document model.
        /// By default, a set of description classes are registered that will
        /// add comments containing within .NET code commit files.
        /// </summary>
        [Fact]
        public void DescriptionImplementations_SpecifiedByDefault()
        {
            
        }

        /// <summary>
        /// The list of registered descriptions can be changed during
        /// bootstrapping of the host.
        /// </summary>
        [Fact]
        public void DefaultDescriptionImplementations_Specified()
        {
            
        }
    }
}
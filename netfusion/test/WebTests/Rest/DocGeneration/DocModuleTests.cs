namespace WebTests.Rest.DocGeneration
{
    /// <summary>
    /// Contains test specific to the proper initialization and execution of the documentation
    /// plugin module.  The module configures documentation related components that are used when
    /// building a document model for a specific WebApi action method.
    /// </summary>
    public class DocModuleTests
    {
        /// <summary>
        /// So other services can obtain a reference to the configuration,
        /// the configuration is exposed as a module property.
        /// </summary>
        public void Module_StoresReference_ToPluginConfiguration()
        {
            
        }

        /// <summary>
        /// The IDocDescription classes, listed by the RestDocConfig class, are
        /// invoked to add additional details to the document model.  These classes
        /// are registered as services so they can have other services injected.
        /// </summary>
        public void Module_RegistersDescriptions_AsServices()
        {
            
        }

        /// <summary>
        /// Some items such as Embedded Resources and Links don't have corresponding
        /// documentation that can easily be specified within the .NET code generated
        /// XML files.  These additional comments are stored within a JSON file.
        /// XML files.  These additional comments are stored within a JSON file.
        /// </summary>
        public void AdditionalComment_AreLoaded_FromJsonFile()
        {
            
        }
    }
}
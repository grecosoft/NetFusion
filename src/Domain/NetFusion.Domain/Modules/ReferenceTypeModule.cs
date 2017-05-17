using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.ReferenceTypes;
using NetFusion.Domain.ReferenceTypes.Core;

namespace NetFusion.Domain.Modules
{
    /// <summary>
    /// Module that configures the needed components of the 
    /// reference type implementation.
    /// </summary>
    public class ReferenceTypeModule : PluginModule
    {
        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            builder.RegisterType<NullReferenceTypeRepository>()
                .As<IReferenceTypeRepository>()
                .SingleInstance();
        }

        // Load the meta-data configured that with enumerated types
        // and associate with base ReferenceType so it can be queried
        // at runtime.
        public override void StartModule(ILifetimeScope scope)
        {
            var referenceTypeRep = scope.Resolve<IReferenceTypeRepository>();
            ReferenceType.SetReferenceTypes(referenceTypeRep.GetReferenceTypes());
        }
    }
}

//using NetFusion.Bootstrap.Plugins;
//using NetFusion.Utilities.Validation;
//using NetFusion.Utilities.Validation.Configs;
//using NetFusion.Utilities.Validation.Core;
//using System;

//namespace NetFusion.Utilities.Modules
//{
//    /// <summary>
//    /// Plug-in module containing validation configurations.
//    /// </summary>
//    public class ValidationModule : PluginModule,
//        IValidationModule
//    {
//        private ValidationConfig ValidationConfg { get; set; }

//        public override void Configure()
//        {
//            this.ValidationConfg = this.Context.Plugin.GetConfig<ValidationConfig>();
//        }

//        public IObjectValidator CreateValidator(object obj)
//        {
//           return (IObjectValidator)Activator.CreateInstance(this.ValidationConfg.ValidatorType, obj);
//        }
//    }
//}

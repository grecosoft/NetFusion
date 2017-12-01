using Autofac.Builder;
using NetFusion.Bootstrap.Plugins;
using System;

namespace NetFusion.Bootstrap.Extensions
{
    public static class AutofacRegistrationExtensions
    { 
        // Extension method that checks the object upon activation to see
        // if it supports the IComponentActivated interface.  If supported,
        // the object being activated is notified.
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> 
            NotifyOnActivating<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder)); 

            return builder.OnActivating(e =>
            {
                (e.Instance as IComponentActivated)?.OnActivated();
            });
        }
    }
}

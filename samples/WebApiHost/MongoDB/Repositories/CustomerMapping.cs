using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiHost.MongoDB.Models;

namespace WebApiHost.MongoDB.Repositories
{
    /// <summary>
    /// Data model mapping.
    /// </summary>
    public class CustomerMapping : EntityClassMap<CustomerModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CustomerMapping()
        {
            this.CollectionName = "RefArch.Customers";
            this.AutoMap();

            MapStringPropertyToObjectId(c => c.CustomerId);
        }

        public override void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {
            var customers = pluginTypes.Where(t =>
                t.IsConcreteTypeDerivedFrom<CustomerModel>());

            customers.ForEach(s => this.AddKnownType(s));
        }
    }

    public class PreferredCustomerMapping : EntityClassMap<PreferredCustomerModel>
    {
        public PreferredCustomerMapping()
        {
            this.AutoMap();
            this.SetDiscriminator("fred");
        }
    }
}

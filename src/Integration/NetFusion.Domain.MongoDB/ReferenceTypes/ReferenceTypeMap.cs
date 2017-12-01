using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.ReferenceTypes;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Domain.MongoDB.ReferenceTypes
{
    public class ReferenceTypeMap : EntityClassMap<ReferenceType>
    {
        public ReferenceTypeMap()
        {
            this.AutoMap();

            MapStringPropertyToObjectId(r => r.ReferenceTypeId);
        }

        public override void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {
            var referenceTypes = pluginTypes.Where(t =>
                t.IsConcreteTypeDerivedFrom<ReferenceType>() && !t.GetTypeInfo().IsGenericType);

            referenceTypes.ForEach(s => this.AddKnownType(s));

        }
    }
}

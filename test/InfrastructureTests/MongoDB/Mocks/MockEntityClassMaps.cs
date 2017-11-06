using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfrastructureTests.MongoDB.Mocks
{
    public class MockEntityClassMap : EntityClassMap<MockEntity>
    {
        public override void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {
            var appSettings = pluginTypes.Where(t =>
                t.IsDerivedFrom<MockEntity>());

            appSettings.ForEach(s => this.AddKnownType(s));
        }
    }

    public class MockDerivedEntityClassMap : EntityClassMap<MockDerivedEntity>
    {
        public MockDerivedEntityClassMap()
        {
            this.SetDiscriminator("expected_descriminator_name");
        }
    }
}

using System.Collections.Generic;
using NetFusion.Base.Scripting;
using NetFusion.Roslyn.Testing;
using Service.Domain.Entities;

namespace Service.App.Services
{
    public static class InMemoryScripting
    {
        public static IEntityScriptingService LoadSensorScript()
        {
            var expressions = new List<EntityExpression>();
            
            expressions.AddExpression("Entity.IsActiveAlert = Entity.CurrentValue < _.MinTemp || Entity.CurrentValue > _.MaxTemp");
            expressions.AddExpression("IsFreezing", "Entity.CurrentValue < _.FreezingTemp");
            expressions.AddExpression("OfficeClosed", "_.IsFreezing && Entity.IsActiveAlert");
            expressions.AddExpression("SomeCalc", "System.Math.Cos((double)Entity.CurrentValue)");
            expressions.AddExpression("AnotherCalc", "_.SomeCalc > .5 ? \"yes\" : \"no\"");
                
            // Create a IEntityScriptingService for a single entity for examples.
            // Note: usually a service will have multiple scripts for different entities
            // and usually loaded from an external source.
            return expressions.CreateService<Sensor>(
                new Dictionary<string, object>
                {
                    {"MinTemp", 65M},        // These are the default attribute values if not specified on the entity. 
                    {"MaxTemp", 82M},
                    {"DesiredTemp", 72M},
                    {"FreezingTemp", 50M}
                });

        }
    }
}
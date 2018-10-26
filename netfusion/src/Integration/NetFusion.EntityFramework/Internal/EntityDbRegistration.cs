using System;
using System.Linq;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.EntityFramework.Internal
{
    /// <summary>
    /// Class used to manage the metadata associated with database
    /// context classes found when the plugin bootstrapped.
    /// </summary>
    public class EntityDbRegistration
    {
        // The IEntityDbContext derived application specific type for which the
        // context instance should be added to the service collection.
        public Type ServiceType { get; }
        
        // The EntityDbContext derived application specific type implementing
        // the service.
        public Type ImplementationType { get;  }

        public EntityDbRegistration(Type implementationType)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            
            if (! IsEntityDbType(implementationType))
            {
                throw new InvalidOperationException(
                    "Type does not meet the constraints of an Entity DB Context.");
            }

            ImplementationType = implementationType;
            ServiceType = implementationType.GetInterfacesDerivedFrom<IEntityDbContext>().First();
        }

        // Determines if a type meets the criteria for a database-context that
        // can automatically be configured by the plugin.
        public static bool IsEntityDbType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            return type.IsConcreteTypeDerivedFrom(typeof(EntityDbContext)) &&
                   type.GetInterfacesDerivedFrom<IEntityDbContext>().Count() == 1;
        }
    }
}
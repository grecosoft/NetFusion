using System.Collections.Generic;
using Demo.App.Entities;
using Nelibur.ObjectMapper;
using NetFusion.Mapping;
using WebApiHost.Models;

namespace WebApiHost.Mappings
{
    public class EntityMappingStrategyFactory : IMappingStrategyFactory
    {
        static EntityMappingStrategyFactory()
        {
            TinyMapper.Bind<Car, CarSummary>();
        }

        public IEnumerable<IMappingStrategy> GetStrategies()
        {
            return new IMappingStrategy[] {
                new MappingDelegate<Car, CarSummary>(TinyMapper.Map<CarSummary>)
            };
        }
    }
}

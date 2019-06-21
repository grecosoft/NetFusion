using System.Collections.Generic;
using Demo.Domain.Entities;
using Demo.WebApi.Models;
using Nelibur.ObjectMapper;
using NetFusion.Mapping;

namespace Demo.WebApi.Mappings
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

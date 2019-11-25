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
            yield return DelegateMap.Map((Car c) => TinyMapper.Map<CarSummary>(c));
        }
    }
}

using System;
using Demo.Domain.Services;

namespace Demo.App.Services 
{
    public class EntityIdGenerator : IEntityIdGenerator
    {
        public string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

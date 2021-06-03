using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.DB.InMemory
{
    public class InMemoryDbFactory : IDatabaseClientFactory
    {
        private readonly Dictionary<string, InMemoryClient> _dbs = new();

        public ValueTask<IDatabaseClient> CreateDatabaseClient(DbClientConfig config)
        {
            if (!_dbs.TryGetValue(config.ConnectionString, out var client))
                _dbs.Add(config.ConnectionString, client = new());

            return ValueTask.FromResult((IDatabaseClient)client);
        }
    }
}

using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.DB.Postgres
{
    public class PostgresDatabaseFactory : IDatabaseClientFactory
    {
        public ValueTask<IDatabaseClient> CreateDatabaseClient(DbClientConfig config)
            => ValueTask.FromResult((IDatabaseClient)new NpgsqlClient(config.ConnectionString));
    }
}

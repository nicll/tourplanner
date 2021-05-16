using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.DB.Postgres
{
    public class PostgresDatabaseFactory : IDatabaseClientFactory
    {
        public async ValueTask<IDatabaseClient> CreateDatabaseClient(DbClientConfig config)
            => new PostgresDatabase(); // ToDo: just a placeholder for now
    }
}

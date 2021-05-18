using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DB.Postgres
{
    public class PostgresDatabase : IDatabaseClient
    {
        public async Task<ICollection<Tour>> QueryTours()
        {
            return Array.Empty<Tour>();
        }

        public async Task BatchSynchronize(IReadOnlyCollection<Tour> newTours, IReadOnlyCollection<Tour> removedTours, IReadOnlyCollection<Tour> changedTours)
        {

        }
    }
}

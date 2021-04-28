using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DB.Postgres
{
    public class PostgresDatabase : IDatabaseClient
    {
        public Task AddTour(Tour tour)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Tour>> QueryTours()
        {
            throw new NotImplementedException();
        }

        public Task RemoveTour(Tour tour)
        {
            throw new NotImplementedException();
        }
    }
}

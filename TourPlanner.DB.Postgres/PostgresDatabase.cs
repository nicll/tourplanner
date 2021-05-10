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

        public async Task AddTour(Tour tour)
        {

        }

        public async Task UpdateTour(Tour tour)
        {

        }

        public async Task RemoveTour(Tour tour)
        {

        }

        public async Task SynchronizeTours(ICollection<Tour> tours)
        {

        }
    }
}

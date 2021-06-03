using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DB.InMemory
{
    internal class InMemoryClient : IDatabaseClient
    {
        private List<Tour> _tours = new();

        public Task<ICollection<Tour>> QueryTours()
            => Task.FromResult((ICollection<Tour>)_tours);

        public Task BatchSynchronize(IChangeTrackingCollection<Tour> tours)
        {
            foreach (var tour in tours.RemovedItems)
                _tours.Remove(tour);

            _tours.AddRange(tours.NewItems);

            // no need to update as tours are directly referenced
            return Task.CompletedTask;
        }
    }
}

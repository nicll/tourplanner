using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDatabaseClient
    {
        Task<ICollection<Tour>> QueryTours();

        Task AddTour(Tour tour);

        Task UpdateTour(Tour tour);

        Task RemoveTour(Tour tour);

        Task SynchronizeTours(ICollection<Tour> tours);
    }
}

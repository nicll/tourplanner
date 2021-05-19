using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDatabaseClient
    {
        Task<ICollection<Tour>> QueryTours();

        Task BatchSynchronize(IChangeTrackingCollection<Tour> tours);
    }
}

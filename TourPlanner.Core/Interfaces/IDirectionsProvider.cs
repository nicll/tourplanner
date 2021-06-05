using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;
using TourPlanner.Core.Exceptions;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// An interface for querying route information.
    /// </summary>
    public interface IDirectionsProvider
    {
        /// <summary>
        /// Loads a <see cref="Route"/> given a starting and an ending point.
        /// If no route could be found, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="from">The starting point.</param>
        /// <param name="to">The ending point.</param>
        /// <returns>Found route or <see langword="null"/>.</returns>
        /// <exception cref="DataProviderExcpetion">An error occured while querying a route.</exception>
        ValueTask<Route> GetRoute(string from, string to);

        /// <summary>
        /// Clears any unnecessary information from the cache.
        /// </summary>
        /// <param name="dataManager">The data manager.</param>
        /// <exception cref="DataProviderExcpetion">An error occured while cleaning the directions cache.</exception>
        ValueTask CleanCache(IDataManager dataManager);
    }
}

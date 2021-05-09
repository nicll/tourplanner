using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

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
        ValueTask<Route> GetRoute(string from, string to);
    }
}

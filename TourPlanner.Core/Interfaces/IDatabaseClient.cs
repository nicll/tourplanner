using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// Defines basic read and write methods for synchronizing data with the database.
    /// </summary>
    public interface IDatabaseClient
    {
        /// <summary>
        /// Read all tours from the database.
        /// </summary>
        /// <returns>A collection of all tours.</returns>
        /// <exception cref="DatabaseException">An error occurs while downloading data.</exception>
        Task<ICollection<Tour>> QueryTours();

        /// <summary>
        /// Writes all current tours to the database.
        /// </summary>
        /// <remarks>
        /// This can either be achieved by dropping all tours in the database and replacing it with the tours returned
        /// from <see cref="IChangeTrackingCollection{T}"/> or by manually adding/removing/updating the changed items.
        /// </remarks>
        /// <param name="tours">A collection tracking the changed items.</param>
        /// <exception cref="DatabaseException">An error occurs while uploading data.</exception>
        Task BatchSynchronize(IChangeTrackingCollection<Tour> tours);
    }
}

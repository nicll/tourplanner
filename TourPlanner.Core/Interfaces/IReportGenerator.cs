using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// An interface for generating reports for tours.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Generate a report for one specific tour.
        /// </summary>
        /// <param name="tour">The tour to base the report on.</param>
        /// <param name="savePath">Where to save the file.</param>
        /// <returns>Awaitable <see cref="Task"/> that may run synchronously.</returns>
        Task GenerateTourReport(Tour tour, string savePath);

        /// <summary>
        /// Generate a report for all given tours.
        /// Includes statistical summary at the end.
        /// </summary>
        /// <param name="tours">All given tours.</param>
        /// <param name="savePath">Where to save the file.</param>
        /// <returns>Awaitable <see cref="Task"/> that may run synchronously.</returns>
        Task GenerateSummaryReport(ICollection<Tour> tours, string savePath);
    }
}

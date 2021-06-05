using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// Provides abstracted ways of communicating with services.
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        /// The currently used <see cref="IDirectionsProvider"/> for querying routes.
        /// </summary>
        IDirectionsProvider CurrentDirectionsProvider { get; }

        /// <summary>
        /// The currently used <see cref="IMapImageProvider"/> for querying map images.
        /// </summary>
        IMapImageProvider CurrentMapImageProvider { get; }

        /// <summary>
        /// The <see cref="IReportGenerator"/> used for generating reports.
        /// </summary>
        IReportGenerator ReportGenerator { get; }

        /// <summary>
        /// The <see cref="IDataConverter"/> used for importing and exporting tours.
        /// </summary>
        IDataConverter DataMigrator { get; }

        /// <summary>
        /// Contains all local tours and keeps track of changes.
        /// </summary>
        IChangeTrackingCollection<Tour> AllTours { get; }

        /// <summary>
        /// Discards all local tours and re-initializes with tours from the database.
        /// Also re-initializes all data providers and clients.
        /// </summary>
        /// <exception cref="TourPlannerException">When any client or data provider failed to initialize.</exception>
        Task Reinitialize();

        /// <summary>
        /// Discards all local tours and re-initializes with tours from the database.
        /// Also re-initializes all data providers and clients with the new <paramref name="config"/>.
        /// </summary>
        /// <param name="config">The new configuration to use for connecting.</param>
        /// <exception cref="TourPlannerException">When any client or data provider failed to initialize.</exception>
        Task Reinitialize(Config config);

        /// <summary>
        /// Pushes all local changes upstream to the database.
        /// </summary>
        /// <exception cref="DatabaseException">When the database could not be synchronized.</exception>
        Task SynchronizeTours();

        /// <summary>
        /// Cleans the cache of all clients and data providers.
        /// </summary>
        /// <returns></returns>
        Task CleanCache();
    }
}

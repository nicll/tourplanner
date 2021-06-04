using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// Provides abstracted ways of communicating with services.
    /// </summary>
    public interface IDataManager
    {
        IDirectionsProvider CurrentDirectionsProvider { get; }

        IMapImageProvider CurrentMapImageProvider { get; }

        IReportGenerator ReportGenerator { get; }

        IDataConverter DataMigrator { get; }

        IChangeTrackingCollection<Tour> AllTours { get; }

        Task Reinitialize();

        Task Reinitialize(Config config);

        Task SynchronizeTours();

        Task CleanCache();
    }
}

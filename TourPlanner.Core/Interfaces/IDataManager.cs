using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDataManager
    {
        IDirectionsProvider CurrentDirectionsProvider { get; }

        IMapImageProvider CurrentMapImageProvider { get; }

        IReportGenerator ReportGenerator { get; }

        ICollection<Tour> AllTours { get; }

        Task Reinitialize();

        Task Reinitialize(Config config);

        Task SynchronizeTours();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.DataManagers
{
    internal class DefaultDataManager : IDataManager
    {
        private Config _currentConfig;
        protected readonly IDataProviderFactory _dpFactory;
        protected readonly IDatabaseClientFactory _dbFactory;
        protected IDirectionsProvider _dpDir;
        protected IMapImageProvider _dpImg;
        protected IDatabaseClient _db;

        public IDirectionsProvider CurrentDirectionsProvider => _dpDir;

        public IMapImageProvider CurrentMapImageProvider => _dpImg;

        public IReportGenerator ReportGenerator { get; }

        public ICollection<Tour> AllTours { get; } = new List<Tour>();

        internal DefaultDataManager(IDataProviderFactory dpFactory, IDatabaseClientFactory dbFactory, IReportGenerator reports)
        {
            _dpFactory = dpFactory;
            _dbFactory = dbFactory;
            ReportGenerator = reports;
        }

        private async Task Initialize(Config config)
        {
            _currentConfig = config;
            _dpDir = await _dpFactory.CreateDirectionsProvider(config.DirectionsApiConfig);
            _dpImg = await _dpFactory.CreateMapImageProvider(config.MapImageApiConfig);
            _db = await _dbFactory.CreateDatabaseClient(config.DatabaseConfig);
        }

        public async Task Reinitialize()
            => await Initialize(_currentConfig);

        public async Task Reinitialize(Config config)
            => await Initialize(config);

        public async Task SynchronizeTours()
            => await _db.SynchronizeTours(AllTours);
    }
}

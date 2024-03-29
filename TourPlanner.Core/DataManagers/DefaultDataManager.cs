﻿using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Internal;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.DataManagers
{
    internal class DefaultDataManager : IDataManager
    {
        private Config _currentConfig;
        private readonly ChangeTrackingCollection<Tour> _tours = new();
        protected readonly IDataProviderFactory _dpFactory;
        protected readonly IDatabaseClientFactory _dbFactory;
        protected IDirectionsProvider _dpDir;
        protected IMapImageProvider _dpImg;
        protected IDatabaseClient _db;

        public IDirectionsProvider CurrentDirectionsProvider => _dpDir;

        public IMapImageProvider CurrentMapImageProvider => _dpImg;

        public IReportGenerator ReportGenerator { get; }

        public IDataConverter DataMigrator { get; }

        public IChangeTrackingCollection<Tour> AllTours => _tours;

        internal DefaultDataManager(IDataProviderFactory dpFactory, IDatabaseClientFactory dbFactory, IReportGenerator reports, IDataConverter migrator)
        {
            _dpFactory = dpFactory;
            _dbFactory = dbFactory;
            ReportGenerator = reports;
            DataMigrator = migrator;
        }

        private async Task Initialize(Config config)
        {
            _currentConfig = config;
            _dpDir = await _dpFactory.CreateDirectionsProvider(config.DirectionsApiConfig);
            _dpImg = await _dpFactory.CreateMapImageProvider(config.MapImageApiConfig);
            _db = await _dbFactory.CreateDatabaseClient(config.DatabaseConfig);

            // get new list and discard changes made during object creation
            var newTours = await _db.QueryTours();
            foreach (var tour in newTours)
                tour.AcceptChanges();

            // clear current list and merge
            _tours.Clear();
            _tours.AcceptChanges();

            // add new tours and merge so they don't duplicate during next sync
            _tours.AddRange(newTours);
            _tours.AcceptChanges();
        }

        public async Task Reinitialize()
            => await Initialize(_currentConfig);

        public async Task Reinitialize(Config config)
            => await Initialize(config);

        public async Task SynchronizeTours()
        {
            if (!_tours.IsChanged)
                return;

            await _db.BatchSynchronize(_tours);
            _tours.AcceptChanges();
        }

        public async Task CleanCache()
        {
            await _dpDir.CleanCache(this);
            await _dpImg.CleanCache(this);
        }
    }
}

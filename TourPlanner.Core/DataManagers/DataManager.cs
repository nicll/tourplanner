using System;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Exceptions;

namespace TourPlanner.Core.DataManagers
{
    /// <summary>
    /// This class is used for initializing data managers.
    /// </summary>
    public static class DataManager
    {
        /// <summary>
        /// Create a <see cref="IDataManager"/> using the supplied configuration and dependencies.
        /// </summary>
        /// <param name="configuration">The configuration used for communicating with external services.</param>
        /// <param name="dataProviderFactory">A factory for creating data providers.</param>
        /// <param name="databaseClientFactory">A factory for creating database clients.</param>
        /// <param name="reportGenerator">A generator for reports.</param>
        /// <param name="dataMigrator">A converter for importing/exporting data.</param>
        /// <returns>An initialized <see cref="IDataManager"/>.</returns>
        /// <exception cref="DataProviderExcpetion">Occurs when initialization of an <see cref="IDirectionsProvider"/>
        /// or <see cref="IMapImageProvider"/> fails.</exception>
        /// <exception cref="DatabaseException">Occurs when querying the database fails.</exception>
        public static async Task<IDataManager> CreateDataManager(Config configuration, IDataProviderFactory dataProviderFactory, IDatabaseClientFactory databaseClientFactory, IReportGenerator reportGenerator, IDataConverter dataMigrator)
        {
            var dataManager = new DefaultDataManager(dataProviderFactory, databaseClientFactory, reportGenerator, dataMigrator);
            await dataManager.Reinitialize(configuration);
            return dataManager;
        }
    }
}

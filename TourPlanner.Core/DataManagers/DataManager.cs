using System;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Configuration;

namespace TourPlanner.Core.DataManagers
{
    public static class DataManager
    {
        public static async Task<IDataManager> CreateDataManager(Config configuration, IDataProviderFactory dataProviderFactory, IDatabaseClientFactory databaseClientFactory, IReportGenerator reportGenerator)
        {
            var dataManager = new DefaultDataManager(dataProviderFactory, databaseClientFactory, reportGenerator);
            await dataManager.Reinitialize(configuration);
            return dataManager;
        }
    }
}

using System;
using System.Threading.Tasks;
using TourPlanner.Converters.Json;
using TourPlanner.Core.DataManagers;
using TourPlanner.Core.Interfaces;
using TourPlanner.DataProviders.MapQuest;
using TourPlanner.DB.InMemory;
using TourPlanner.DB.Postgres;
using TourPlanner.Reporting.PDF;

namespace TourPlanner.GUI
{
    internal static class DependencyInitializer
    {
        public static async Task<IDataManager> InitializeRealDataManager()
            => await DataManager.CreateDataManager(OSInteraction.LoadConfig("connection.config"),
                new MapQuestApiFactory(), new PostgresDatabaseFactory(), new ReportGenerator(), new JsonConverter());

        public static async Task<IDataManager> InitializeDummyDataManager()
            => await DataManager.CreateDataManager(OSInteraction.LoadConfig("connection.config"),
                new MapQuestApiFactory(), new InMemoryDbFactory(), new ReportGenerator(), new JsonConverter());
    }
}

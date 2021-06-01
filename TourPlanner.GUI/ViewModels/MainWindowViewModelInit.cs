using System;
using System.Threading.Tasks;
using TourPlanner.Core.DataManagers;
using TourPlanner.DataProviders.MapQuest;
using TourPlanner.DB.Postgres;
using TourPlanner.Reporting.PDF;

namespace TourPlanner.GUI.ViewModels
{
    public class MainWindowViewModelInit : MainWindowViewModel
    {
        protected event EventHandler InitializationFinished;

        public MainWindowViewModelInit()
        {
            Task.Run(async () =>
            {
                FinishInitialization(await DataManager.CreateDataManager(OSInteraction.LoadConfig("connection.config"),
                    new MapQuestApiFactory(), new PostgresDatabaseFactory(), new ReportGenerator()));
                InitializationFinished?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}

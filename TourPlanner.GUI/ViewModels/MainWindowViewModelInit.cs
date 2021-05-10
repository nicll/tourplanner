using System;
using TourPlanner.DataProviders.MapQuest;
using TourPlanner.DB.Postgres;
using TourPlanner.Reporting.PDF;

namespace TourPlanner.GUI.ViewModels
{
    public class MainWindowViewModelInit : MainWindowViewModel
    {
        public MainWindowViewModelInit()
            : base("connection.config", new PostgresDatabase(), new ReportGenerator(), new MapQuestApiFactory())
        {
        }
    }
}

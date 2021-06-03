using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourPlanner.Core.DataManagers;
using TourPlanner.Core.Models;

namespace TourPlanner.GUI.ViewModels
{
    public class DesignMainWindowViewModel : MainWindowViewModel
    {
        public DesignMainWindowViewModel() : base(null)
        {
            SelectedTour = new Tour(new List<LogEntry> { new() { Date = DateTime.Now, Distance = 1.23, Duration = TimeSpan.FromMinutes(30), Rating = 0.77F }, new() })
            {
                Name = "Beispielstour",
                CustomDescription = "Meine Beschreibung",
                Route = new Route { Steps = new[] { new Step { Distance = 1.23, Description = "Schritt 1" }, new() { Distance = 2.34, Description = "Schritt 2" } } },
                ImagePath = ""
            };
            ShownTours = new() { SelectedTour, new Tour { Name = "Test 2", CustomDescription = "leer", Route = new() { TotalDistance = 0.1, Steps = new[] { new Step(), new Step() } } } };
            Task.Run(async () =>
            {
                FinishInitialization(await DependencyInitializer.InitializeDummyDataManager());
                OverwriteTours(ShownTours);
            });
        }
    }
}

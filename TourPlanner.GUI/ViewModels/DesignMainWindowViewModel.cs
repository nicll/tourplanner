using System;
using System.Collections.Generic;
using TourPlanner.Core.Models;

namespace TourPlanner.GUI.ViewModels
{
    public class DesignMainWindowViewModel : MainWindowViewModelInit
    {
        public DesignMainWindowViewModel()
        {
            SelectedTour = new Tour
            {
                Name = "Beispielstour",
                CustomDescription = "Meine Beschreibung",
                Route = new Route { Steps = new[] { new Step { Distance = 1.23, Description = "Schritt 1" }, new() { Distance = 2.34, Description = "Schritt 2" } } },
                ImagePath = @"C:\Users\Nicolas\Programming\repos\tourplanner\TourPlanner.GUI\bin\Debug\net5.0-windows\TourPlanner_CachedImages\60888fee-0235-4ee4-02b4-3532-0e3fbcda8225.png",
                Log = new List<LogEntry> { new() { Date = DateTime.Now, Distance = 1.23, Duration = TimeSpan.FromMinutes(30), Rating = 0.77F }, new() }
            };
            ShownTours = new() { SelectedTour, SelectedTour, SelectedTour, SelectedTour, new Tour { Name = "Test 2", CustomDescription = "leer", Route = new() { TotalDistance = 0.1, Steps = new[] { new Step(), new Step() } } } };
            UpdateLoadedTours(ShownTours);
        }
    }
}

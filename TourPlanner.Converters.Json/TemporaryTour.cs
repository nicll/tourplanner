using System;
using System.Collections.Generic;
using TourPlanner.Core.Models;

namespace TourPlanner.Converters.Json
{
    internal record TemporaryTour(Guid TourId, string Name, string ImagePath, string CustomDescription, Route Route, List<TempLogEntry> Log);
}

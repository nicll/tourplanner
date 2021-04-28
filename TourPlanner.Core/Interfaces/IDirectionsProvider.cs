using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDirectionsProvider
    {
        ValueTask<Route> GetRoute(string from, string to);
    }
}

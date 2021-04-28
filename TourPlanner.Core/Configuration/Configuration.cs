using System;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Configuration
{
    public class Configuration
    {
        public ApiClientConfig DirectionsApiConfig { get; init; }

        public ApiClientConfig MapImageApiConfig { get; init; }

        public DbClientConfig DatabaseConfig { get; init; }
    }
}

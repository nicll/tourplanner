using System;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Configuration
{
    /// <summary>
    /// Contains the necessary configuration for <see cref="DataManagers"/>.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Configuration regarding the directions querying API.
        /// </summary>
        public ApiClientConfig DirectionsApiConfig { get; init; }

        /// <summary>
        /// Configuration regarding the map imaging API.
        /// </summary>
        public ApiClientConfig MapImageApiConfig { get; init; }

        /// <summary>
        /// Configuration regarding the database.
        /// </summary>
        public DbClientConfig DatabaseConfig { get; init; }
    }
}

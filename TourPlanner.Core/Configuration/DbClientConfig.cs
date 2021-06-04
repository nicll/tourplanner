using System;

namespace TourPlanner.Core.Configuration
{
    /// <summary>
    /// Defines typical configuration needed for communicating with the database.
    /// </summary>
    public class DbClientConfig
    {
        public string ConnectionString { get; init; }
    }
}
